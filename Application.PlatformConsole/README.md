# BUTS Platform Console

Standalone Angular app for the platform operator — **separate** from the tenant
ERP UI (`Application.Client`). It talks only to `/api/platform/*` and signs in
against `PlatformAdmin` accounts, never tenant users.

## Run

```bash
npm install
npm start          # ng serve on http://localhost:4300, proxied to the API on :5254
```

The API must be running (`dotnet run --project ../Application.Api`). On first API
start a seed admin is created from the `PlatformAuth` section of
`Application.Api/appsettings.json` (`SeedEmail` / `SeedPassword`) — change those
before any real deployment.

## Build

```bash
npm run build      # -> dist/platform-console
```

## What's here

| Route | Purpose |
|---|---|
| `/login` | Platform-admin sign-in |
| `/dashboard` | Tenant counts, module adoption, estimated MRR, recent activity |
| `/tenants` | List / search; create a tenant from a template + plan |
| `/tenants/:id` | Status, subscription, module toggles, quota overrides, "act as tenant" token |
| `/plans` | Plans (upsert) and versioned price books (draft → publish) |
| `/modules` | Module catalog + business templates |
| `/audit` | Full platform audit trail |

Roles gate the mutating actions: `Owner` > `Admin` > `Support` > `ReadOnly`
(create/edit needs `Admin`, impersonation needs `Support`).
