# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build

# Run API (from Application.Api directory)
dotnet run --project Application.Api

# Run with watch (hot reload)
dotnet watch --project Application.Api run

# EF Core migrations — code-first since InitialBaseline (dotnet-ef is a local tool; run `dotnet tool restore` first)
dotnet ef migrations add <MigrationName> --project Application.Core --startup-project Application.Api
dotnet ef database update --project Application.Core --startup-project Application.Api

# The entities in Application.Core/Entities/ + Data/Configurations/ are now the
# source of truth. Do NOT re-scaffold with EF Core Power Tools — hand-edit the
# entity/config and add a migration. generic-erp-db was baselined at InitialBaseline
# (an existing __EFMigrationsHistory row, no schema change). efpt.config.json is
# kept for reference only.
```

API runs on `http://localhost:5254` by default. Swagger UI available at `/swagger`.

## Architecture

This is a .NET 9 ERP REST API using **Clean Architecture** with four projects:

| Project | Role |
|---|---|
| `Application.Api` | Controllers, middleware, DI wiring, Swagger |
| `Application.Core` | Entities, EF DbContext, interfaces, enums, settings |
| `Application.Services` | Business logic, DTOs, validators, AutoMapper profiles |
| `Application.Repository` / `Application.Infrastructure` | UnitOfWork, BaseRepository, external services |
| `Application.Client` | Tenant-facing Angular 14 ERP UI |
| `Application.PlatformConsole` | Standalone Angular 20 platform-admin console (separate app, own login) |

**Dependency direction**: Api → Services → Core ← Repository

### Key Patterns

**Generic CRUD via BaseService**: All domain services extend `BaseService<TEntity, TCreationDto, TUpdateDto, TRequestModel, TViewModel>`. It handles `SearchAsync` (filtering/ordering/pagination), `AddAsync`, `UpdateAsync`, `DeleteAsync` with audit tracking. Extend it; don't duplicate.

**Repository + Unit of Work**: `BaseRepository<T>` wraps EF queries. `IUnitOfWork` coordinates transactions and injects `CreatedBy`/`ModifiedBy` audit fields automatically. Use `_unitOfWork.SaveAsync()` to commit.

**Autofac DI**: Services are registered via Autofac modules (one per domain), not `services.AddXxx`. Add new services to the relevant module in `Application.Api/Extensions/AutofacExtension.cs`.

**FluentValidation**: Validators live in `Application.Services/Services/Validators/`. Auto-validation is enabled — invalid requests are rejected before hitting the controller. Use `AbstractValidator<TDto>` and register via the relevant Autofac module.

**AutoMapper profiles**: Five profiles keyed by domain (`ConfigurationProfile`, `PurchaseProfile`, `ProductionProfile`, `SaleProfile`, `AccountProfile`) in `Application.Services`. Add new maps to the appropriate profile.

**Permission-based auth**: Use the custom `[Authorize("PermissionName")]` attribute (not the built-in one). Permissions are seeded and managed via `PermissionHelpers` in `Application.Core`.

**Soft deletes**: Global EF query filters exclude soft-deleted records automatically. Set `Deleted = true`; never hard-delete domain entities (except the types in `UnitOfWork`'s hard-delete list).

**Multi-tenancy (Phase 0 — see `docs/phase0-tenancy.md`)**: Every entity with a `Guid TenantId` is marked `ITenantScoped` in `Application.Core/Interfaces/TenantScopedEntities.cs` and gets an automatic EF query filter (tenant + soft-delete). The current tenant comes from the JWT `tenantId` claim via `TenantScope.CurrentTenantId` (and `ITenantContext` for DI). `UnitOfWork` stamps `TenantId` on insert and throws on any cross-tenant modify/delete. Do **not** hand-write `WHERE TenantId`. `BaseRepository.TableUnfiltered()` bypasses both filters — pre-auth paths only. `IWorkContext` still exposes `TenantId`/`UserId` for services.

**Shared rows (`ITenantSharable`)**: `Account` implements `ITenantSharable : ITenantScoped` — its query filter matches `TenantId == current OR TenantId == TenancyConstants.SystemTenantId` (`ffffffff-…`). The standard chart-of-accounts skeleton (the ~46 `AccountHeadConstants` GUIDs the accounting engine hard-codes, plus the 10 intermediate group accounts up to the roots — 55 rows) carries `SystemTenantId`, so every tenant resolves the same head GUIDs while its own customer/supplier/expense accounts stay tenant-scoped. `UnitOfWork` never re-stamps or cross-tenant-guards a `SystemTenantId` row. `UQ_Account_Code` is unique per `(TenantId, Code)`. Migration `ShareSystemAccountHeads`. New tenants get working accounting reports with no seed step (they see the shared skeleton).

**Platform layer (Phase 1 — see `docs/saas-platform-plan.md`)**: `Application.Core/Entities/Platform/*` — `PlatformModule` catalog (PK `Key`, seeded via `HasData`), `TenantModule` (per-tenant on/off), `BusinessTemplate` (`pharmacy`/`feed`, both enable all modules — industry is a behaviour profile, never a module gate), `Subscription`, `Entitlement`, `TenantSetting`. `TenantResolutionMiddleware` (after `JwtMiddleware`) loads the enabled modules / template / subscription status / quotas into `HttpTenantContext` (`ITenantContext`) per request. `GET /api/me` returns that state for the Angular shell. `[RequiresModule("<key>")]` (in `Application.Api/Attributes`) gates a controller/action behind a module — throws `ModuleNotEnabledException` (body `statusCode` 403) before permission checks; applied to all purchase/sales/production/inventory/accounts/report controllers, not to Configuration/master-data/auth. Existing tenants are put on the platform layer by the `BackfillExistingTenants` migration.

**Industry profile (Phase 1 — replaces `BusinessType` branching)**: `Application.Core/Industry/` — `IIndustryProfile` with `PharmacyProfile` / `FeedProfile`. Registered per request in `InfrastructureModule` as `IndustryProfiles.For(ITenantContext.BusinessTemplateKey)`. Inject `IIndustryProfile`; never write `if (BusinessType == …)`. Members:
- `Uom.ToSellable(primaryQty, product)` — Feed `× Product.BagWeight` (Kg), Pharma identity.
- `Sales` — `RequireApprovedCustomerDiscountOnOrder` (Feed), `OneInvoicePerSaleOrder` / `BlockInvoiceUnpostWhenReceiptExists` (Pharma), `CollectionReportGroupsByZone` (Feed).
- `Reports` — `CompactLayout` (Feed → A5 docs / A5 footer / portrait dense list reports; Pharma → full page) and `ProductLabel(name, code, packSize)` (Pharma includes pack size).
- `Dashboard` — `ShowQuantityKpis` (Feed) / `ShowValueKpis` (Pharma).

All business-logic `BusinessType` branches are migrated (sale order/invoice services, `AccountService`/`AccountReportPdfService` collection report, `DashboardService`, the report controllers, `ConfigurationPdfService` product list, `PurchasePdfService` logo). Still on `BusinessType` as a deliberate compat shim: the `businesstype` JWT claim (`JwtMiddleware`/`AuthService`), `Tenant.BusinessType` column, and `ClaimsHelper.GetAllPermissions` (industry permission catalog at login — becomes a module `PermissionGroup` later). Also deferred: the `* BagWeight` expressions inside EF `Select` projections (`SaleOrderService` report queries, `DeliveryNoteService`) need query restructuring. See `docs/saas-platform-plan.md` §11.

**Platform-admin surface (Phase 1 — plan §07)**: a second auth surface, kept apart from tenant auth. `PlatformAdmin` account store; `PlatformAuthService` issues a token with a `scope=platform` claim and **no** tenant id; `PlatformAuthMiddleware` validates it (its own `PlatformAuth` appsettings section / audience) and fills `IPlatformAdminContext` **without** touching `TenantScope`. `[PlatformAuthorize(minRole)]` (`Owner`>`Admin`>`Support`>`ReadOnly`) gates `/api/platform/*`. Platform services (`Application.Services/Services/Platform/`) inject `DataContext` directly (never `IUnitOfWork` — its `OnBeforeSaveChangesAsync` requires a tenant) and use `IgnoreQueryFilters()` for cross-tenant reads; every mutation appends a `PlatformAuditLog` row. New schema: `PlatformAdmin`, `Plan`, `PriceBook`+`PriceBookEntry`, `PlatformAuditLog` (migration `AddPlatformAdminAndBilling`). First API start seeds an admin from `PlatformAuth:SeedEmail`/`SeedPassword`. Impersonation (`/tenants/{id}/impersonate`) issues a tenant token via `IAuthService.IssueTokensForUserAsync`, audited. The console UI is the separate `Application.PlatformConsole` app (`npm install && npm start`, proxies `/api` to :5254).

### Domain Areas

The API covers: **Auth**, **Configuration** (lookups/masters), **Inventory** (products, stock), **Purchase** (PO, GRN), **Sale** (SO, delivery notes), **Production** (BOM, manufacturing orders), **Accounts** (chart of accounts, journal entries, vouchers, fund transfers), and **Reports** (COGS, ledgers, stock analysis).

### Startup Flow

```
Program.cs
  └── RegisterServices()       → appsettings binding, EF, Autofac, Serilog, CORS, JWT, 
  |                               rate limiting, AutoMapper, FluentValidation, Swagger
  └── SetupMiddleware()        → GlobalExceptionMiddleware → JwtMiddleware → HTTPS → CORS
                                  → static files → Serilog request logging → rate limiting → routing
```

### Cross-Cutting Infrastructure

- **Logging**: Serilog with SQL Server sink; request logs include `UserId`, `TenantId`, `Domain` context.
- **Error handling**: `GlobalExceptionMiddleware` catches all unhandled exceptions and returns a standardized `ErrorResult`.
- **Rate limiting**: 10 requests / 10 seconds per IP for `/api/*` (via `AspNetCoreRateLimit`).
- **Exports**: EPPlus for Excel, iTextSharp for PDF (purchase orders, delivery notes, BOM).
- **External services**: `MailService` (Gmail SMTP via MailKit), `SmsService` (external HTTP API), `ApiCaller` (generic HTTP wrapper).

### Configuration

Key sections in `appsettings.json`:
- `ConnectionStrings.DefaultConnection` — SQL Server (dev: `203.76.124.70`, db: `ERPDB_DEV`)
- `JwtSettings` — `Secret`, `Issuer`, `Audience`, `ExpiryMinutes` (1440), `RefreshTokenExpirationDays` (30)
- `MailSettings` — Gmail SMTP credentials
- `CacheSettings` — 12h absolute / 60m sliding
- `IpRateLimiting` — rate limit rules
- `AllowedCorsOrigins` — `localhost:4200`, `https://pharma.butsbd.com`
