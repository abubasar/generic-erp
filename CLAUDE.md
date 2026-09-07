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
