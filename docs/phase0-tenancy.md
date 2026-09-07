# Phase 0 — Tenancy foundation

Hardened, fail-closed tenant isolation in `Application.Api`. This is the
prerequisite for the platform layer (see `saas-platform-plan.md` §08).

## What it does

| Concern | Mechanism |
|---|---|
| Who is the tenant? | JWT `tenantId` claim → `JwtMiddleware` → `TenantScope.CurrentTenantId` (an `AsyncLocal<Guid>` in `Application.Core/Common/TenantScope.cs`). Also exposed via DI as `ITenantContext` / `HttpTenantContext`. |
| Reads are scoped | Every `ITenantScoped` entity gets an EF Core query filter `e => e.TenantId == <current> && !e.Deleted`, applied in `Application.Core/Data/DataContext.Tenancy.cs`. The tenant value is baked into each `DataContext` via its tenant-aware constructor and parameterised per query. |
| No tenant in scope | Tenant-scoped queries return **nothing** (fail closed). Writes throw `UnauthorizationException`. |
| Writes are scoped | `UnitOfWork.OnBeforeSaveChangesAsync` stamps `TenantId` on inserts and **throws on any modify/delete of a row whose stored `TenantId` ≠ current** — catches update-by-Id / `Attach` against another tenant, which the read filter cannot stop. |
| `FindAsync(id)` | `DbSet.Find` bypasses query filters, so `BaseRepository.FindAsync(Guid)` re-checks the tenant and treats another tenant's row as not-found. |
| Bypassing the filter | `IBaseRepository.TableUnfiltered()` (`IgnoreQueryFilters()`, drops tenant **and** soft-delete). Pre-auth paths only — login, refresh token, tenant lookup/provisioning. Every caller constrains the query by username / token / explicit `TenantId`. |

## Marking a new entity as tenant-scoped

Add a line to `Application.Core/Interfaces/TenantScopedEntities.cs`:

```csharp
public partial class YourEntity : ITenantScoped { }
```

The entity must already have a non-nullable `Guid TenantId` column. That's it —
the query filter and save guard pick it up by the interface. Regenerate the full
list any time with:

```bash
grep -l "public Guid TenantId { get; set; }" Application.Core/Entities/*.cs
```

## Re-scaffolding the DbContext (EF Core Power Tools)

A re-scaffold regenerates `Application.Core/Data/DataContext.cs` and everything
under `Application.Core/Entities/`. It will **not** touch:

- `Application.Core/Data/DataContext.Tenancy.cs` (the tenant filter + constructor)
- `Application.Core/Interfaces/TenantScopedEntities.cs` (the `ITenantScoped` markers)
- `Application.Core/Data/DataContext.cs`'s partner file for SP results

After a re-scaffold:

1. Rebuild. `DataContextConstructionTests` must still pass — it proves DI still
   selects the tenant-aware constructor over the regenerated single-arg one.
2. Run `dotnet test` — `TenantIsolationTests` + `TenantWriteGuardTests` must pass.
3. If new tenant tables were added, add their `ITenantScoped` lines (above).

## Database indexes

`docs/sql/2026-09-07_tenant-id-indexes.sql` — idempotent script adding a leading
`TenantId` index to every tenant-scoped table (only 1 of 110 had one). Review and
run against each environment; replace single-column indexes with composites on
hot tables.

## Deferred to Phase 1

- **Host / sub-domain tenant resolution.** `JwtMiddleware` only reads the JWT
  claim. Sub-domain fallback needs `Tenant.Subdomain`, which does not exist yet
  (the `Tenant` table is extended in Phase 1).
- **Background-job tenant scope.** Hangfire jobs run with no `TenantScope`, so
  tenant-scoped queries return nothing. Fine for now (loud failure, not a leak);
  revisit with the metering/billing jobs.
- `ITenantContext` grows enabled modules / quotas / subscription state.
