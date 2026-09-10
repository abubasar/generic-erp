# From two products to one ERP SaaS platform

**Architecture & Migration Plan — v1.1 draft**

How to turn the existing Feed ERP and Pharmacy POS into a multi-tenant,
template-driven ERP that a business owner can sign up for, configure by
answering plain questions, and pay for by what they switch on.

| | |
|---|---|
| **Scope** | `GenericERP` (the `src` repo — .NET `Application.Api` + Angular `Application.Client`, formerly BUTSERP_API + ERPAngular) · ButsPosDotnet6 · PMSAdminReact · PMSShopAngular |
| **Prepared** | 2026-09-06 · rev. 2026-09-06 (repo consolidation; verified against code + local `generic-erp-db`) |
| **Target** | Modular monolith · pooled multi-tenancy |
| **Interactive version** | https://claude.ai/code/artifact/52c83647-f44f-433b-ae51-3a21d4950465 |

---

## Contents

- [Start here — the plain version](#start-here--the-plain-version)
- [00 · Executive summary](#00--executive-summary)
- [01 · What exists today](#01--what-exists-today)
- [02 · Target architecture](#02--target-architecture)
- [03 · Key architectural decisions](#03--key-architectural-decisions)
- [04 · Module access vs user permissions](#04--module-access-vs-user-permissions)
- [05 · Generic vs industry-specific](#05--generic-vs-industry-specific)
- [06 · Onboarding & pricing experience](#06--onboarding--pricing-experience)
- [07 · Platform administration](#07--platform-administration)
- [08 · Migration roadmap](#08--migration-roadmap)
- [09 · Frontend consolidation](#09--frontend-consolidation)
- [10 · The twelve questions, answered](#10--the-twelve-questions-answered)
- [11 · Risks & guardrails](#11--risks--guardrails)
- [12 · Requirements coverage check](#12--requirements-coverage-check)
- [Appendix A · Platform data model sketch](#appendix-a--platform-data-model-sketch)

---

## Start here — the plain version

The rest of this document is the detailed architecture. This section is the
whole idea in plain language, and a step-by-step way to get there. Read this
first.

### What you have today

- Two separate products — the **Feed ERP** (which already serves *two*
  industries, Pharmaceutical and Feed, from one codebase) and the **Pharmacy
  POS**.
- Every new customer means a developer sets up a database, deploys a copy, and
  configures it by hand.
- How fast you can grow is capped by how many installations your team can look
  after.

> **A head start you already built:** the Feed ERP is *not* single-industry.
> Every tenant carries a `BusinessType` — **1 = Pharmaceutical**, **2 = Feed** —
> and the app already changes its behaviour per tenant: which permissions load,
> which dashboard figures show, which invoice / delivery-note / production PDF
> layout prints, even different business rules (Feed sales require an approved
> customer-wise discount; Pharmaceutical don't). So "one app, many business
> types" is already proven in your code — this plan formalises the pattern so a
> *third* and *fourth* industry don't each mean editing 30 files. See §01 and
> §05.

### What you want

One website. A shop owner signs up on their own, answers a few simple
questions, sees a price, and starts using the system the same day — with no
developer involved. Think **"Shopify, but for ERP"**: Shopify never builds a new
app per store; every store is just data inside one system. Same idea here.

### The core concept

One application runs everything. Each customer is a **tenant** — a walled-off
space inside that one app. A pharmacy and a feed mill use the same software but
can never see each other's data.

Two independent switches decide what a person sees on screen:

1. **Did the business pay for this?** — their subscription decides which
   **modules** are on (POS, Production, Batch tracking…). Applies to everyone in
   that business.
2. **Is this staff member allowed?** — the owner sets **permissions** per role
   (a cashier can sell but not change prices). Applies to one person.

You can use a feature only when **both** switches are on. Behind the scenes it
is still your existing, tested code — configuration only flips switches; it does
not build the ERP out of a database.

> **Why this is achievable:** your Feed ERP backend is already about **70% built
> for this** — it already tags every record with a tenant and carries the tenant
> identity in the login token. The missing piece is the "business layer": plans,
> modules, subscriptions, pricing, and the signup wizard.

### Step by step to get there

Each stage delivers something real, and you never stop serving current
customers.

#### Stage 0 — Decide and prepare

*Goal: agree the target; nobody writes tenant code yet.*

- Lock the big decisions (all recommended in §03): one app + one shared
  database; modules are code with on/off flags; price = base + modules + per
  user / branch / terminal; templates, not customer-built workflows.
- Choose the backend that *becomes* the platform — the Feed ERP API
  (`Application.Api`). It and its Angular admin UI (`Application.Client`) already
  live in one repo (`GenericERP`); everything else folds into that.
- Set up a staging environment, a CI pipeline, and a rule: every new table
  starts with `TenantId`.

**Done when** the team agrees on the target and staging is running.

#### Stage 1 — Make the ERP genuinely tenant-safe

*Goal: no customer can ever see another's data — provably.*

- Add one central "who is this tenant" object filled from the login token on
  every request.
- Replace the hand-written "filter by tenant" code (repeated in many places
  today, easy to forget) with **one automatic rule** applied to every query.
- Add a safety catch: saving any record without a tenant is a hard error.
- Write automated tests — log in as Tenant A, try to read Tenant B's data,
  assert it is impossible — for every screen.

**Done when** tests prove no tenant can reach another's data.

#### Stage 2 — Add the business layer

*Goal: plans, modules and subscriptions exist; your team manages tenants from
one screen.*

- Create the new tables: `Tenant`, `Module` catalog, `TenantModule`, `Plan`,
  `PriceBook`, `Subscription`, `Entitlement` (limits).
- Fill the module catalog from what already exists — Inventory, Purchase,
  Sales, Production, Accounting…
- Build a small **internal admin console** (your team only): create a tenant,
  toggle modules, set a price.
- Move your current live Feed customers in — each becomes a tenant on the Feed
  template with an active subscription.

**Done when** existing customers run as tenants, managed from one console.

#### Stage 3 — Put a switch on every feature

*Goal: turning a module off makes it disappear for that tenant.*

- Add a check to every part of the API: "is this module on for this tenant?" If
  not → a clear "not on your plan" message.
- Build the app menu **from the tenant's active modules**, not a hardcoded list.
- When a tenant admin sets up staff roles, show only permissions for modules
  they actually own.

**Done when** a module toggled off in the console vanishes from that tenant's
app.

#### Stage 4 — Self-service signup (first public launch)

*Goal: a stranger signs up for a Feed ERP with no developer involved.*

- Build the **wizard**: account → business details → 6–8 plain questions →
  recommended setup with editable tick-boxes → live price → confirm.
- Build **provisioning**: on confirm, auto-create the tenant, switch on chosen
  modules, load starter data (chart of accounts, units, one branch, owner
  login). Must be safely re-runnable if it fails halfway.
- Put a simple marketing / signup site in front.
- Invoicing stays **manual** here — your team issues the bill and marks it paid.

**Done when** someone signs up and is working in ~10 minutes, untouched by your
team. *This is the SaaS milestone.*

#### Stage 5 — Bring in the Pharmacy POS

*Goal: one platform, not two backends.*

- Port the POS logic (counter sale, cash drawer, returns) from the old .NET 6
  POS into a **POS module**.
- Port batch / expiry tracking into a **Pharmacy module**.
- Port the POS terminal screen from the Angular shop app into the main app as a
  "terminal mode".
- Migrate existing pharmacy customers — each old database becomes one tenant.
- Switch off the old POS backend; retire the old React admin app.

**Done when** a pharmacy can sign up and run their counter entirely inside the
new platform.

#### Stage 6 — Automate the money

*Goal: subscriptions renew and collect themselves.*

- Track usage (users, branches, terminals) automatically each month.
- Auto-generate invoices; handle mid-month plan changes.
- Add online payment — bKash / SSLCOMMERZ for Bangladesh, card for others.
- Add automatic dunning: payment fails → grace period → read-only → suspended,
  with emails.
- Let tenants upgrade / downgrade themselves.

**Done when** billing runs with nobody lifting a finger.

#### Stage 7 — Grow the range

*Goal: a new industry is days of config, not a new project.*

- Add Super Shop, Wholesale, Retail, Buying House as **new templates** — reuse
  existing modules, build only the small industry-specific piece each genuinely
  needs.
- Offer a dedicated database as an Enterprise upgrade for large customers.

**Done when** adding a business type is template + config work only.

### If you only start with three things

1. **Stage 1** — automatic tenant filtering + isolation tests. The foundation,
   and the biggest risk right now.
2. **Stage 2** — the module / subscription tables + internal admin console.
3. **Stage 3** — feature switches + a menu built from active modules.

Those three turn the Feed ERP into a real multi-tenant platform. Stage 4 then
makes it a *SaaS*.

---

## 00 · Executive summary

You are not building a generic ERP. You are building a configurable ERP SaaS
where a tenant tells you about their business, gets a recommended setup, sees a
price, and starts working — with no developer in the loop per customer.

The good news from the code review: **the Feed ERP backend (`Application.Api`) is
already ~70% of the way to the right architecture** — and it now shares one repo
(`GenericERP`) with its Angular client (`Application.Client`), so "one codebase"
has already begun. It is a .NET 9 modular monolith with per-domain Autofac
modules, a `TenantId` discriminator column on nearly every entity, a tenant
claim carried in the JWT, audit + soft-delete + tenant stamping centralised in
`UnitOfWork`, and — crucially — it **already runs two industries from one codebase**: a
per-tenant `BusinessType` (`Primary` = 1 = Pharmaceutical, `Secondary` = 2 =
Feed) drives a three-way permission catalog (shared `Permissions` +
`PrimaryPermissions` + `SecondaryPermissions`) and ~32 runtime branches for
dashboards, PDF layouts, unit conversion and business rules. What it lacks is the *platform layer* — module catalog,
subscriptions, entitlements, pricing, business templates, self-service
onboarding — and the industry split needs to move off scattered `if` statements
onto that module layer.

> **The recommendation in one line:** make the **Feed ERP backend
> (`Application.Api`, in the `GenericERP` repo) the platform**. Add a
> Platform module (tenants, modules, subscriptions, pricing, templates). Convert
> the manual `WHERE TenantId` filtering to EF global query filters that fail
> closed. Gate every existing domain behind a module flag. Fold the Pharmacy POS
> in as a **POS + Pharmacy module** rather than running a second backend. Ship
> the **Feed** and **Pharmacy** templates first; add Super Shop, Wholesale and
> Buying House by composing existing modules.

### What we are deliberately not doing

- No microservices. One deployable, one database (with a dedicated-DB escape
  hatch for Enterprise).
- No no-code / dynamic-form engine. Business logic stays in code — *stable code
  + configurable features*.
- No free-form workflow builder for customers. Templates + module toggles +
  settings only, at least for v1–v2.
- No per-tenant branches of code. Ever.
- No payment-gateway integration on day one. Manual invoicing first; automate
  billing once tenants exist.
- No feature locked to Pharma or Feed. Both industries reuse every existing
  domain — the industry is a behaviour profile (units, report layout, dashboard
  metric, one sales rule), not an entitlement. See §05.

---

## 01 · What exists today

Originally five repositories. The Feed ERP backend and its Angular admin UI have
now been merged into a **single repo — `GenericERP`, the `src` scope** (.NET
solution `Application.Api` + Angular `Application.Client`), so what follows is
four repos, two backends, three frontends. Two different and incompatible ideas
of multi-tenancy are still in play.

| Repo / project | Stack | Role | Tenancy model today | Disposition |
|---|---|---|---|---|
| **`GenericERP` → `Application.Api`** (was BUTSERP_API) | .NET 9 · EF Core · Autofac · Clean Arch (Api / Core / Services / Repository + Infrastructure) | **Two-industry** distribution / manufacturing ERP backend — purchase, production (BOM, MO), sales, inventory, full accounting, ~77 controllers, ~200 services. Serves **Pharmaceutical** (`BusinessType 1`) and **Feed** (`BusinessType 2`) tenants from the same code. | **Row-level.** `TenantId` Guid on entities; value from JWT claim → `HttpContext.Items`; `BaseRepository.TableNoTracking()` appends `WHERE TenantId=` by hand; `UnitOfWork` stamps tenant + audit + soft-delete + `EventLog`. Global query filter only on `Deleted`. Dormant DB-per-tenant path via subdomain → `__DBNAME__`. | Evolve → platform core |
| **`GenericERP` → `Application.Client`** (was ERPAngular) | Angular 14.2 · MatX template · Material · SignalR · Node 16 | ERP admin UI — views split by domain (accounts, configuration, dashboard, inventory, production, purchase, sales, report) + sessions/utilities | Single deployment. Menu is a 523-line hardcoded array in `navigation.service.ts`, each item carrying a `permission` string, filtered in the sidenav by an `*appHasPermission` structural directive; per-route `canMatch: hasPermission([...])` guards; API URL hardcoded in `config.ts` (re-exported through `environments/environment.ts`). Decodes a `businesstype` JWT claim and branches on it in **315 places across 109 component files**. No `/api/me` / profile call — `checkTokenIsValid()` returns a hardcoded `DEMO_USER`. | Evolve → tenant shell |
| **ButsPosDotnet6** | .NET 6 · EF Core (scaffolded, `GHP_POS_DBContext`) · permission-policy auth (`[Authorize(Policy = Permissions.X.Y)]` via a custom `PermissionPolicyProvider`) · no Autofac · Api / Core / Repository / Services + RequestModel / ViewModel | Pharmacy POS backend — fast sale (`Sale`/`SaleItem`), sale returns, purchase, multi-branch + branch transfers, batch/expiry stock, customer payments, lightweight accounting (journal, account heads, expenses, fiscal year); CQRS-lite (generic `BaseCommandService` / `BaseQueryService` called straight from ~40 controllers; only ~7 real services) | **Silo.** One DB (`GHP_POS_DB`) via `DefaultConnection`; a separate deployment + subdomain per customer (CORS: ghppos, vetmedpos, demopos, vet, ghp, demo…). `AddClientDbContext` (Referer sub-domain → `__DBNAME__`, strips "pos") exists but is **commented out**. `Branch` entity handles multi-location *within* one customer. **String PKs everywhere. No `TenantId`. No query filters at all — not even `Deleted`.** | Port → POS + Pharmacy modules |
| **PMSAdminReact** | React 16.8 · MUI v4 · react-scripts 3 (via `react-app-rewired` / `config-overrides.js`) · formik · MatX React template (`matx-react` 2.0.0) · redux present but **only MatX boilerplate reducers** (Ecommerce/Nav/Notification/ScrumBoard) — no domain state | Pharmacy POS back-office UI — **not small**: Configuration (~40 files: Branch, Category, Product, Supplier, Customer, User, Role, PaymentMethod, MeasurementUnit, Generic, FiscalYear, ExpenseType, Message, notification), Transaction (~46: Purchase, PurchaseReturn, Sale, CustomerPayment, SupplierPayment, Expense, DamageLost, StockAdjustment, StockAdjustmentBatch, Transfer/Detail/Multiple), Accounts (AccountHead, JournalEntry, PayReceive), Dashboard, and a **split `Report` vs `LegacyReports`** (ItemWise*, Stock, SaleReturn… vs BalanceReport, CustomerLedger, SupplierLedger, ProfitLoss). ~380 source files, ~54 routes. | Single deployment per customer; `.env`-per-build API URL (the checked-in `CopyOfDotEnv` rotates demopos / ghp / vetmed API hosts). | Retire — dead-end stack |
| **PMSShopAngular** | Angular 12 (scaffold from 11) · Material · module + layout architecture · hash routing | **Staff POS terminal** (not a customer shop) — `modules/{admin,pos,home,auth}` + matching `layouts/`, but only `pos` is real: `home` is a one-page welcome, `admin` is an empty stub. 18 of 33 components are the terminal: catalog → cart → discount (flat/%) → cash / card / credit payment → checkout, plus customer selection & registration. One `PosComponent` composes them; online-only (no service worker / offline store); no shift / cash-drawer. | Single deployment per customer; hardcoded `environment.ts` API URL. | Port `modules/pos` + `pos-layout` → terminal mode in tenant shell |

> **The core tension to resolve:** the ERP already runs **pooled** (many
> tenants, one DB, row filter). The POS runs **siloed** (one deploy per
> customer). You cannot keep both. The plan standardises on **pooled**, because
> that is what makes self-service signup, central module management and
> usage-based billing possible — a new tenant is a row, not a deployment.

### Assets worth reusing as-is

- **Permission model** — `[Authorize("Permissions.X.Y")]` + `RoleClaims` table +
  reflection-based permission catalog, already business-type aware
  (`ClaimsHelper.GetAllPermissions(this List<RoleClaimModel>, int businessType)`
  adds the shared `Permissions` set plus *either* `PrimaryPermissions` (pharma)
  *or* `SecondaryPermissions` (feed) by reflecting over their nested types).
- **Per-industry behaviour switching** — the tenant's `BusinessType` is a JWT
  claim (`enum BusinessType { Primary = 1 /*Pharmaceutical*/, Secondary = 2
  /*Feed*/ }`); ~32 sites in services and report controllers branch on it for
  dashboard KPIs, PDF layouts (paired `PrintSaleOrderPrimaryReportToPdfAsync` /
  `…SecondaryReportToPdfAsync` methods — also for DeliveryNote, SaleInvoice,
  SaleReturn), and rules like Feed's mandatory customer-wise discount. The
  *mechanism* for industry modules already exists — it just needs formalising.
- **Dual unit-of-measure model** — every sale-detail table already carries a
  `Primary*` quantity (bags, as entered) and a plain quantity (the sellable
  unit): for Feed the plain quantity is `Primary × Product.BagWeight` (Kg); for
  Pharma it is a straight copy of the `Primary*` value. See §05.
- **Audit pipeline** — `OnBeforeSaveChangesAsync` writes `EventLog` with old/new
  JSON, stamps tenant + audit fields, and does per-type hard vs soft delete.
- **Generic CRUD** —
  `BaseService<TEntity,TCreationDto,TUpdateDto,TRequestModel,TViewModel>` with
  search / filter / order / paginate.
- **Accounting engine** — chart of accounts, journal entries, vouchers, fund
  transfers, COGS reporting. This is the hardest thing to build and it already
  exists.
- **POS domain logic** — batch/expiry stock (`StockItem.BatchNo` / `.ExpiryDate`;
  `Product.IsExpiryItem`; `StockAdjustmentBatch`), COGS-per-sale carried on the
  data (`Sale.Cogs`, `SaleItem.CogsPerItem`, `SaleReturn.TotalCogsOfReturnItems`),
  branch transfers (`Transfer` / `TransferMultiple`), customer payments/dues.
  Note: there is **no shift / cash-drawer / register** entity in the POS backend
  — the "terminal" is `Sale` + `SaleItem` only.

> **Two things are *not* as the earlier draft implied.** (1) The `Tenant` table
> **already exists** (Id, Code, Name, TimeZoneId, Address, ContactNo, BINNo,
> Email, `BusinessType` int, Logo + audit + `Deleted`) — the platform work
> *extends* it, it is not a new table. (2) A stale **net6.0** copy of
> `BaseRepository.cs` / `UnitOfWork.cs` lives in an orphan `Application.Infrastructure/`
> folder that is **not in `Application.sln`**; the live code is
> `Application.Repository/Application.Infrastructure.csproj` (net9.0, namespace
> `Application.Infrastructure`). Delete the orphan before it causes an edit in
> the wrong file.

---

## 02 · Target architecture

A single modular-monolith .NET application. One deployable unit, internally
organised into layers whose boundaries are enforced by project references and
Autofac modules — the structure `Application.Api` already has, plus a Platform layer
above it and Industry modules beside the business modules.

```
Platform layer            cross-tenant · admin-only
  Tenant management · Business templates · Module catalog ·
  Subscriptions & entitlements · Pricing engine · Usage metering ·
  Onboarding / signup · Billing & invoices
        │  resolves ITenantContext for every request
        ▼
Core ERP                  generic · always on
  Identity & roles · Organisation / branch · Product / item · Customer ·
  Supplier · Settings · Numbering & documents · Reporting framework

Business modules          generic · toggled per tenant
  Inventory / stock · Purchase (PO · GRN) · Sales (SO · invoice · delivery) ·
  POS · Accounting · Pricing & discounts · Barcode / labels · Advanced reporting

Industry modules          specific · one per business type
  Pharmacy   — batch, expiry, drug schedule
  Feed       — formula/BOM, manufacturing orders, by-products
  Super Shop — promotions, shelf, weigh-scale
  Buying House — style costing, export docs
  Wholesale  — routes, credit control
```

### How a request is scoped

One piece of middleware turns an incoming request into a fully-resolved tenant
context before any controller runs. Everything downstream — query filters,
module gates, quota checks — reads from it.

1. **Identify tenant** — JWT `tenantId` claim (app) or sub-domain / custom
   domain (marketing & POS terminal).
2. **Load context** — from cache: tenant status, enabled module keys, quotas,
   subscription state, DB connection key, settings.
3. **Bind DbContext** — pooled connection by default; swap to the tenant's
   dedicated connection if one is set.
4. **Filter every query** — EF global query filter on `ITenantScoped.TenantId`;
   `SaveChanges` guard rejects any scoped entity missing a tenant.
5. **Gate the endpoint** — `[RequiresModule]` / `[RequiresFeature]` / quota
   filters check entitlements; subscription state can force read-only.

> **Concrete first change:** grow `ITenantContext` (scoped) from the existing
> `IWorkContext` / `WorkContext` (already injected into ~every controller) and
> the `ClaimsHelper` `HttpContext` extensions, and add an `ITenantScoped` marker
> interface. Move the `WHERE TenantId` logic out of
> `BaseRepository.TableNoTracking()` (and delete the `TableWithoutTenant()`
> bypass) into `modelBuilder` global query filters applied to every
> `ITenantScoped` entity. Today `UnitOfWork.OnBeforeSaveChangesAsync` throws when
> there is *no* ambient tenant, but on `Modified`/`Deleted` it simply stamps
> `TenantId = current tenant` without checking the row already belonged to that
> tenant — so an update-by-Id across tenants silently succeeds. This removes an
> entire class of "someone forgot to filter" data-leak bugs and is a mechanical,
> testable change.

### Configuration lives at three levels

Nothing about a tenant's setup is generated code — it is all rows hanging off
the tenant, read into `ITenantContext` at request time.

```
Platform
 │
 ├── Module catalog ........ every module that exists, its category & dependencies
 ├── Business templates .... Pharmacy · Feed · Super Shop · Wholesale · Retail · Buying House · Other
 ├── Plans & price book .... Starter / Business / Enterprise + per-module / per-unit prices
 │
 └── Tenant  (one row per customer)
      ├── Subscription ...... plan or "build your own", status, locked price, period
      ├── Business type ..... which template it was created from
      ├── Enabled modules ... TenantModule rows  (bought / not)
      ├── Entitlements ...... max users · branches · POS terminals · storage
      ├── Users & roles ..... tenant-scoped; each role = a set of permissions
      ├── Branches .......... organisation units, up to quota
      └── Settings .......... typed key/value: currency, timezone, numbering, tax, toggles
```

### How it scales

Pooled tenancy scales *up* cheaply and *out* when it has to:

- **Stateless app tier** — no session state; scale horizontally behind a load
  balancer. `ITenantContext` is rebuilt per request from a short-TTL cache
  (Redis in production, in-memory today).
- **Database** — one primary carries many tenants; add read replicas for
  reporting queries; move the heaviest / largest tenants to a dedicated database
  via `DbConnectionKey` without any code change.
- **Background work** — provisioning, metering, invoicing, PDF/Excel exports and
  emails run on a queue, not in the request.
- **Noisy-neighbour control** — the existing per-IP rate limiting becomes
  per-tenant; heavy exports are throttled per plan tier.

---

## 03 · Key architectural decisions

Six decisions carry the design. Each is stated as a verdict with the reasoning
and the escape hatch. A seventh — the split between what a tenant *bought* and
what a user *may do* — is important enough to get its own section (§04).

### D1 · Multi-tenancy strategy

**Verdict:** one application instance, **pooled** tenancy, row-level isolation by
`TenantId`, enforced by EF global query filters. Tenant resolved from JWT claim
(app) or host (public surfaces).

This is where `Application.Api` already is — the work is hardening it, not replacing
it. A separate app/instance per tenant (today's POS model) makes self-service
signup, central upgrades and cross-tenant analytics all but impossible. Pooled
means a new customer is an `INSERT`.

**Escape hatch:** a per-tenant `DbConnectionKey`. Null = shared pool. Set =
dedicated database, provisioned from a template. Sold as an Enterprise option
for tenants with data-residency or scale needs. The subdomain → `__DBNAME__`
plumbing for this already exists in `TenantDataContextExtension`.

### D2 · Shared database vs database per tenant

**Verdict:** **shared database by default** (pool model). Dedicated database
available as a per-tenant override (bridge model). **Never** schema-per-tenant.

Shared keeps operational cost flat and migrations single-shot. Row-level filters
+ a fail-closed `SaveChanges` guard + a dedicated tenant-isolation integration
test suite are the safety net. Schema-per-tenant multiplies migration pain by
the tenant count for no real isolation gain over a discriminator column.

**Indexing:** every tenant-scoped table needs a leading `TenantId` in its main
indexes. Today this is effectively absent — of ~110 tenant-scoped tables, exactly
**one** (`ReceivePayment`, via `IX_ReceivePayment_TenantId_Deleted`) has a
`TenantId`-leading index and only one other (`Stock`, `idx_stock_tenantid`) has
any `TenantId` index at all. Treat this as a from-scratch pass plus a scaffolding
rule, not a tidy-up.

### D3 · Module activation

**Verdict:** modules are **code**, compiled in. Activation is **data**: a
platform `Module` catalog (seeded) plus per-tenant `TenantModule` rows with
status and optional expiry.

Enforced in three places, defence in depth:

1. **API** — a `[RequiresModule(ModuleKeys.Pos)]` authorization filter on
   controllers/actions, reading `ITenantContext.Modules`.
2. **Navigation** — the Angular shell builds its menu from
   `GET /api/me/modules` instead of the hardcoded `navigation.service.ts` array,
   and the existing `*appHasPermission` directive gains a sibling entitlement
   check. (There is no `/api/me` today — the client never fetches a profile.)
3. **Service layer** — guard clauses on cross-module calls.

Modules declare dependencies (`Pos` needs `Inventory` + `Sales`); activation
resolves them. The generic `Permissions.AccessModules.*` constants already in
the codebase become the seed for this catalog.

### D4 · Subscription enforcement

**Verdict:** middleware loads the active `Subscription` + `Entitlements` into
`ITenantContext` (cached ~60 s). Attributes and create-time checks read from it.
Expiry degrades gracefully through a grace period.

| State | Behaviour |
|---|---|
| Trial | Full access to template modules; countdown shown in shell. |
| Active | Access = enabled modules ∩ paid entitlements. |
| PastDue | Grace window (e.g. 7 days): full access + banner. Then → read-only. |
| Suspended | Login allowed; all writes blocked; export + billing pages only. |
| Cancelled | Data retained 90 days for reactivation, then purged. |

**Quotas** (max users, branches, POS terminals, storage) are checked at the
point of creation — a clear "upgrade to add another branch" message, not a
silent failure.

### D5 · Pricing calculation

**Verdict:** a deterministic `PricingEngine`:
`total = planBase + Σ modulePrice + Σ (meteredQty × unitPrice)`. Prices come
from a **versioned price book**; a tenant keeps the version they signed up on
until they actively change plan.

Keep the model boring and legible: monthly, in BDT. **Ship with three metered
dimensions** — additional users, additional branches, additional POS terminals —
because those are the ones tenants understand and can predict. Storage and
"advanced features" stay in the price-book *schema* (any SKU can be metered) but
are not billed until there is a real reason to; storage overage in particular
waits for Phase 4 when you can actually measure it. Everything else is included
in a module's flat price. The wizard shows a live line-item quote as the user
toggles.

**Plans** (Starter / Business / Enterprise) are just named bundles: a set of
included module keys + included quotas + a base price. "Build your own" is the
same engine with an empty bundle. One code path, not two. See §06 for the plan
table and the build-your-own flow.

### D6 · Business configuration

**Verdict:** **template-driven.** A `BusinessTemplate` (Pharmacy, Feed, Super
Shop, Wholesale, Retail, Buying House) maps to recommended module keys + default
settings + a **seed pack** (chart of accounts, units, roles, document
numbering). A plain-language questionnaire adjusts the recommendation.

Settings are stored as typed setting classes backed by a `TenantSetting`
key/value table. (The existing `Setting` table is *not* tenant-scoped today —
it has no `TenantId` column — so this either adds one or replaces it.) Reads go
through strongly-typed accessors, never raw strings scattered in services.

The customer can override the recommendation before finishing, and change most
toggles later from Settings. What they cannot do is invent new document types or
workflows — that stays in code. *Stable code, configurable features.*

---

## 04 · Module access vs user permissions

These are two independent questions and the architecture must never conflate
them.

### Axis 1 — Module access: *did the tenant buy this?*

A property of the *tenant's subscription*. Set by the platform: template + plan
+ build-your-own choices + admin overrides → `TenantModule` rows and
`Entitlement` limits. Changes when they upgrade, downgrade, or a payment lapses.
Applies to every user in the tenant equally.

*Enforced by* `[RequiresModule]` · `[RequiresFeature]` · quota checks · menu
from `/api/me`.

### Axis 2 — User permission: *may this person do it?*

A property of the *user's role within the tenant*. Set by the tenant admin:
roles (Owner, Sales Manager, Cashier, Accountant…) each hold a set of permission
strings in `RoleClaims`. The Cashier can use POS but not Purchase; the Sales
Manager can see customers but not System Settings.

*Enforced by* the existing `[Authorize("Permissions.Sales.Create")]` attribute +
`RoleClaims` table.

> **Effective access = the intersection.** A user can reach a feature only if
> **the tenant has the module** *and* **the user's role has the permission**.
> Module gate is checked first (cheaper, coarser, returns "not on your plan");
> permission gate second (returns "ask your administrator"). The permission
> catalog is filtered to a tenant's enabled modules, so a tenant admin setting
> up roles only ever sees permissions for modules they actually own — they never
> assign a permission for POS if POS is off.

The codebase already has the raw material for both axes:
`Permissions.AccessModules.*` constants map cleanly onto Axis 1's module keys,
and the per-feature `Permissions.<Area>.<Action>` constants + `RoleClaims` are
Axis 2 as-is. The new work is the tenant/subscription layer that decides which
`AccessModules` a tenant gets, and filtering the role-setup screen by it.

---

## 05 · Generic vs industry-specific

> **Pharma and Feed reuse 100% of the existing features.** This is a hard
> constraint, not an aspiration: every domain the ERP has today — configuration,
> inventory, purchase, production/BOM/MO, sales, full accounting, every report —
> is used by **both** industries. No existing feature is, or becomes, locked to
> one of them. For these two, "industry" is a **behaviour profile** (units,
> report layout, which dashboard metric, one sales rule) layered over the shared
> feature set — never an entitlement that turns a feature on or off. Keep this
> true as the code is refactored: an `IIndustryProfile` call is fine; a
> `[RequiresModule("Feed")]` gate on an existing screen is not.

The dividing line for *new* verticals: if two business types would both want a
capability and mean the same thing by it, it is **generic** (a shared Core or
Business module). If it only makes sense for one industry, or the same word means
different things, it is an **industry module** — but that granularity is for
Super Shop, Buying House and the like, not for splitting today's shared ERP.

> **You already have the two-industry mechanism — as `if` statements.** The Feed
> ERP's `BusinessType` split (`Primary = 1` = Pharmaceutical, `Secondary = 2` =
> Feed) drives ~32 `if (BusinessType == …)` branches plus permission-visibility
> subsets — `PrimaryPermissions` (8 nested groups: Territories, PackSizes,
> ProductAudits, primary sales & inventory report modules…) and
> `SecondaryPermissions` (5 nested groups: customer-wise discounts, delivery
> notes, receive payments…). These subsets **hide/show parts of shared features
> per industry; they are not separate feature sets.** Formalising means: (1) each
> permission class becomes a permission *group* surfaced by the industry profile
> (not a paid module); (2) each `if` branch becomes a call into an injected
> `IIndustryProfile` chosen by the tenant's template; (3) the paired
> `…Primary…` / `…Secondary…` PDF methods become the profile's report pack. Do
> this *before* adding Super Shop, or industry #3 means editing all 32 sites
> again — see §08 Phase 1 and §11.

### The dual unit-of-measure model (feed vs pharma)

The single biggest behavioural difference between the two industries is already
in the schema, on every sale-detail table (`SaleQuotationDetail`,
`SaleOrderDetail`, `DeliveryNoteDetail`, `SaleInvoiceDetail`, `SaleReturnDetail`).
Each line stores **two** quantities:

| Field family | Meaning | Feed | Pharma |
|---|---|---|---|
| `Primary*` (`PrimaryQuantity`, `PrimaryBonusQuantity`, `DeliveredPrimaryQuantity`, `ReturnPrimaryQuantity`, …) | what the user enters — **bags** | as entered | as entered |
| plain (`Quantity`, `BonusQuantity`, `DeliveryQuantity`, `ReturnQuantity`, …) | the sellable/reportable unit | `Primary × Product.BagWeight` → **Kg** | straight **copy** of the `Primary*` value |

`Product.BagWeight` (int, Kg per bag) is the conversion factor. The conversion is
done today in scattered service code (e.g. `DeliveryNoteService` computes
`DeliveryQuantity + DeliveryPrimaryBonusQuantity × BagWeight`). Under the plan
this belongs in the industry profile — an `IUomPolicy` on `IIndustryProfile`:
Feed's implementation multiplies by `BagWeight`, Pharma's is identity. Note the
quantity columns are all `int`, so partial bags truncate — a real precision
decision to make when the feed module is formalised.

> **`SaleReturnDetail`** additionally has newly-added bonus fields
> (`PrimaryBonusQuantity`, `ReturnPrimaryBonusQuantity`, `BonusQuantity`,
> `ReturnBonusQuantity`) following the same rule.

| Capability | Layer | Source today | Notes |
|---|---|---|---|
| Identity, roles, permissions | Core | `Application.Api` | Keep the `RoleClaims` model; add platform-admin roles above tenant roles. |
| Unit-of-measure conversion (bags ⇄ Kg) | Industry (profile) | `Application.Api` (scattered) | `Primary*` = bags; plain qty = Kg for Feed (`× Product.BagWeight`) / copy for Pharma. Move to `IUomPolicy` on `IIndustryProfile`. |
| Organisation, branches, warehouses | Core | Both (ERP `Store`, POS `Branch`) | Unify `Branch`/`Store` into one org-unit concept. |
| Product / item master | Core | Both | Superset of fields; industry modules add extension tables (drug schedule, formula flag). |
| Customer, supplier | Core | Both | ERP models these as `Account` sub-types; reconcile with POS's flat `Customer`. |
| Inventory & stock ledger | Business | Both | Batch/expiry-aware valuation from the POS; weighted-average COGS. |
| Purchase — PO, GRN, returns, LC | Business | `Application.Api` (richer) | LC / import costing is optional sub-feature. |
| Sales — SO, invoice, delivery note | Business | `Application.Api` | Credit limit / discount validation already present. |
| POS — fast sale, returns, customer payments | Business | ButsPosDotnet6 + PMSShopAngular | Port the fast-sale flow, per-line COGS, returns, customer dues; the `PMSShopAngular` terminal (catalog/cart/discount/cash-card-credit/checkout) is the UI reference. **Shift / cash-drawer / register exists in neither the backend nor the terminal — design it fresh.** No offline mode today either. |
| Accounting — CoA, journals, vouchers | Business | `Application.Api` (full) | The keystone asset. POS's light ledger retires into this. |
| Barcode / label printing | Business | New (thin) | Optional module; super shop & pharmacy retail. |
| Advanced reporting / analytics | Business | `Application.Api` report controllers | Metered/priced tier. |
| Production — BOM, manufacturing orders, raw material | Business | `Application.Api` (Production module) | **Used by both** pharma and feed today (pharma reports value, feed reports qty) — a shared Business module, *not* feed-only. |
| **Pharmacy profile** — expiry/near-expiry emphasis, drug schedule, A5 invoice layout | Industry (profile + optional sub-features) | `Application.Api` (`Primary` branches) + ButsPosDotnet6 (batch/expiry) | Behaviour + report pack over the shared features; batch/expiry tracking is an optional sub-feature any tenant can enable. |
| **Feed profile** — bags↔Kg conversion, by-product yield, mandatory customer-wise discount, feed report layouts | Industry (profile) | `Application.Api` (`Secondary` branches) | Pure behaviour over the shared features — units, one sales rule, report layouts. No feature is feed-exclusive. |
| **Super Shop** — promotions, combo/gift items, shelf, weigh-scale | Industry | Effectively new (POS has only `GifItem`: `Id·Name·Quantity·PurchaseItemId` — a gift-with-purchase line, no promo engine) | New thin module over POS + Barcode. |
| **Buying House** — style costing, order tracking, export documentation | Industry | New | Reuses Purchase + Sales + Accounting; adds style/costing entities only. The *manufacturing* cousin — **Garments / RMG** (merchandising + costing + LC/trade + TNA planning + shop-floor production) — is a much larger vertical with an existing source system to port; see `docs/garments-vertical-plan.md`. |
| **Wholesale** — routes/territory, van sales, credit control | Industry | `Application.Api` (territory, MO-wise collection) | Mostly config over Sales; small module. |

### Adding a new business type later

1. Analyse the requirement against the capability table above.
2. Create a `BusinessTemplate` row + seed pack + questionnaire branch.
3. Reuse Core + whichever Business modules fit.
4. Write *only* the missing Industry module — extension entities, a handful of
   services, a few screens, a report pack.
5. Add its price-book entries. Ship.

No forking, no per-customer code. A "Restaurant" or "Auto Workshop" template is a
weekend of template + seed work plus however much genuinely new domain logic it
needs.

> **A large vertical is not a weekend.** The "genuinely new domain logic" clause
> dominates for a manufacturing vertical. **Garments / RMG** — merchandising,
> costing, trade finance (LC/BB-LC), TNA planning and shop-floor production are
> four domains this platform does not have. Bringing in the existing `SCERP`
> garments ERP (~437 controllers, ~1,050 EF6 entities, ~2,900 views, ~307 report
> definitions; minus HRM/Payroll and Accounting — Accounting reuses this same
> Feed-ERP module) is a **~16–26 engineer-month** re-platform, not a template
> tweak — see **`docs/garments-vertical-plan.md`** (v1.1, from a full source read).

---

## 06 · Onboarding & pricing experience

The customer's whole experience is: **tell us about your business → get a
recommendation → choose what you need → see your price → start.** No ERP
vocabulary on screen.

| Step | What the customer does | Behind the scenes |
|---|---|---|
| 1 · Account | Name, email, password | Creates the owner user + an `OnboardingSession`. No tenant yet. |
| 2 · Business | Business name, type, country, currency, time zone | Country/currency/TZ default from IP; type drives everything next. |
| 3 · Questions | 6–8 plain questions | Answers map to module toggles. |
| 4 · Review | "Your recommended setup" + editable toggles + live price | Line-item quote updates on every change. |
| 5 · Start | Confirm → provision → getting-started checklist | Tenant + seed pack created; owner lands in the app. |

### Questionnaire → configuration mapping

| Plain question | If yes |
|---|---|
| Do you sell directly to walk-in customers? | + POS |
| Do you have more than one location? | + Multi-branch, quota > 1 |
| Do you make or assemble products yourself? | + Production / Formula |
| Do you track expiry dates? | + Expiry management |
| Do you track stock by batch or lot? | + Batch management |
| Do you scan barcodes at the counter? | + Barcode / labels |
| Do you give credit / track customer dues? | + Receivables, credit limits |
| Do you import goods under LC? | + Import & LC costing |

### Template defaults

| Template | Modules seeded on |
|---|---|
| **Pharmacy** | Inventory · Purchase · Sales · POS · Batch · Expiry · Accounting |
| **Feed industry** | Inventory · Purchase · Sales · Production/Formula · Raw material · Accounting |
| **Super shop** | Inventory · Purchase · POS · Barcode · Promotions · Accounting |
| **Wholesale** | Inventory · Purchase · Sales · Routes · Credit control · Accounting |
| **Retail** | Inventory · Purchase · Sales · POS · Accounting |
| **Buying house** | Purchase · Sales · Style costing · Export docs · Accounting |
| **Other** | Inventory · Purchase · Sales · Accounting (questionnaire fills the rest) |

### What the review step looks like

Modules are shown in two groups, in plain terms, with the recommendation
pre-ticked and the price moving as the customer changes anything.

```
Your recommended setup — Pharmacy                    ৳ 4,500 / month
─────────────────────────────────────────────────────────────────
ALWAYS INCLUDED                          included in base
  ✓ Track stock              ✓ Purchases      ✓ Sales & customers
  ✓ Money in / money out (accounting)

CHOOSE WHAT YOU NEED
  ☑ Counter sales (POS)                        + ৳ 1,200
  ☑ Track batch / lot numbers                  + ৳   400
  ☑ Track expiry dates                         + ৳   400
  ☐ Barcode & label printing                   + ৳   300
  ☐ Advanced reports & analytics               + ৳   800

YOUR TEAM & LOCATIONS
  Users                3   (2 included, +1 × ৳ 250)       + ৳ 250
  Branches             1   (1 included)                        —
  POS terminals        2   (1 included, +1 × ৳ 350)      + ৳ 350
─────────────────────────────────────────────────────────────────
  Base ৳ 2,000 + modules ৳ 2,000 + team ৳ 600  =  ৳ 4,600 / month
  14-day free trial · cancel anytime · change your plan later
```

### Predefined plans

| Plan | Included |
|---|---|
| **Starter** (small shop) | Stock · Purchase · Sales · Accounting · 2 users · 1 branch. No POS. |
| **Business** (growing) | Everything in Starter + POS + Barcode + Advanced reports · 8 users · 3 branches · 2 POS terminals. |
| **Enterprise** (large / group) | All modules · custom user & branch counts · optional dedicated database · priority support. |

### Build your own plan

1. Pick business type → gets a template recommendation.
2. Tick / untick modules.
3. Set number of users.
4. Set number of branches (and POS terminals).
5. See the calculated monthly price, itemised.
6. Start the free trial → subscribe.

Same `PricingEngine`, same screen — a "plan" just pre-fills steps 2–4.

### Provisioning (must be idempotent)

On confirm, a single transactional job: create `Tenant` + `Subscription` (Trial)
→ assign subdomain → create `TenantModule` rows from the final selection → run
the seed pack (CoA, units, roles, numbering, one branch, financial year) →
create the owner's tenant user with the Owner role → send welcome email. If it
fails partway, it can be safely re-run — key every step on `(TenantId, stepKey)`.

> **Language discipline:** ban these words from customer-facing screens —
> *module, entity, permission, claim, discriminator, tenant, GRN, voucher,
> ledger posting*. Say "track stock", "your team", "what you can do", "goods
> received", "money in / money out". Keep the ERP terms for the platform-admin
> console and the developer docs.

---

## 07 · Platform administration

A separate console — separate login, separate role hierarchy, never mixed into a
tenant's UI. It operates *above* tenants and its own requests are not
tenant-scoped (they explicitly select a tenant or run cross-tenant).

### Platform admin can

- Create, suspend, reactivate, delete tenants
- Edit the module catalog & dependencies
- Publish price-book versions & plans
- Override a tenant's modules / quotas / price
- See usage, MRR, churn, provisioning failures
- Impersonate a tenant user for support (audited)
- Manage business templates & seed packs
- Move a tenant to a dedicated database

### Tenant admin (the Owner role) can

- Manage their team, roles & what each person can do
- Turn optional modules on/off (price updates live)
- Add branches / users up to quota, or upgrade
- Edit business settings & document templates
- See their own invoices & usage
- Export their data

> **Keep them apart in code:** two authorization surfaces — `[PlatformAuthorize]`
> (checks a platform-admin identity, ignores tenant context) and the existing
> `[Authorize("Permissions…")]` (tenant-scoped). A platform admin acting on a
> tenant does so through an explicit, audited "act as tenant X" path — not by
> holding tenant claims.

---

## 08 · Migration roadmap

Incremental. Existing Pharmaceutical and Feed ERP customers keep working
throughout — they are already tenants with a business type, so they become
tenants #1..n on their templates. No big-bang rewrite. Each phase ships
something usable.

### Phase 0 — Tenancy foundation  ·  *largely done — see `docs/phase0-tenancy.md`*

*Goal — one hardened, fail-closed isolation model in `Application.Api`.*

- ✅ `ITenantScoped` marker on the 109 tenant entities + `ITenantContext`;
  ambient `TenantScope` (`AsyncLocal`) set by `JwtMiddleware`.
- ✅ Automatic EF query filter (tenant + soft-delete) on every `ITenantScoped`
  type in a re-scaffold-safe `DataContext` partial; `BaseRepository` no longer
  hand-writes `WHERE TenantId`; `TableWithoutTenant()` → `TableUnfiltered()`
  (pre-auth paths only).
- ✅ `UnitOfWork` fail-closed write guard: throws on any `Modified`/`Deleted`
  scoped entity whose stored `TenantId` ≠ current; `FindAsync(id)` re-checks
  (it bypasses filters).
- ✅ Isolation test suite (`Application.Tests`, xUnit + EF InMemory) — reads
  scoped, writes blocked, `FindAsync` isolated, DI picks the tenant-aware ctor.
- ✅ Deleted the orphan net6 `Application.Infrastructure/` project.
- ◻️ Leading `TenantId` index on every scoped table —
  `docs/sql/2026-09-07_tenant-id-indexes.sql` ready; run per environment.
- ◻️ Host / sub-domain tenant resolution — deferred to Phase 1 (needs
  `Tenant.Subdomain`). JWT-claim path is live.
- ◻️ End-to-end (`WebApplicationFactory`, real HTTP, every endpoint) isolation
  tests — the InMemory suite covers the mechanism; add HTTP-level coverage
  alongside Phase 1's second seeded tenant.

### Phase 1 — Platform layer + module gating

*Goal — modules and subscriptions exist; internal admin console.*

- New `Platform` Autofac module: **extend** the existing `Tenant` entity
  (add `Subdomain`, `BusinessTemplateKey`, `Status`, `Currency`,
  `DbConnectionKey?`, …); add `BusinessTemplate`, `Module`, `TenantModule`,
  `Plan`, `PriceBook`, `Subscription`, `Entitlement`, `TenantSetting`.
- Seed the module catalog from existing `Permissions.AccessModules.*` (the seven
  shared domains — Configuration, Purchase, Production, Inventory, Sales,
  Accounts, Report — all of which **both** pharma and feed get). `PrimaryPermissions`
  / `SecondaryPermissions` become the **Pharmacy / Feed industry-profile
  permission groups** — visibility subsets *within* those shared modules, not
  separate paid modules.
- **Retire `BusinessType` branching.** Introduce `IIndustryProfile` (resolved
  from the tenant's template) and replace the ~32 `if (BusinessType == …)` sites
  with a module check or a profile call; keep the `businesstype` JWT claim and
  `Tenant.BusinessType` column only as a compatibility shim during the
  transition.
- `[RequiresModule]` / `[RequiresFeature]` filters; apply to all existing
  controllers.
- `GET /api/me` returns modules + quotas + subscription state.
- Internal platform-admin console (Angular) — tenants, modules, pricing,
  overrides.
- Backfill: existing **Pharmaceutical** and **Feed** tenants onto their
  templates with an Active subscription (they already carry the right
  `BusinessType`).

### Phase 2 — Self-service onboarding — Feed template GA

*Goal — a stranger can sign up for a Feed ERP and pay.*

- Marketing + signup site (lightweight, separate app).
- 5-step wizard, questionnaire, live pricing quote, "build your own".
- Idempotent provisioning job + seed packs.
- Angular tenant shell: menu from `/api/me`, subscription banners, quota
  prompts, self-serve module toggles.
- Manual invoicing (platform admin issues invoices; mark paid).

### Phase 3 — POS + Pharmacy modules — Pharmacy template GA

*Goal — the Pharmacy POS lives inside the platform, not as a second backend.*

- Port POS domain from ButsPosDotnet6 → `Pos` business module. This is a
  **re-platform, not a lift**: .NET 6→9, string PK→Guid, add `TenantId` + the
  global query filters, silo→pool, *and* adopt the ERP's patterns it lacks —
  Autofac per-domain module, `BaseService`, `UnitOfWork` audit/soft-delete
  pipeline (the POS has none of these; ~40 controllers call generic
  command/query services directly, no `Deleted` filter).
- Design the missing **POS till session** (shift / cash-drawer / register) model
  — it exists in neither the POS backend nor the `PMSShopAngular` terminal.
  (The ERP's `Shift` table is *employee scheduling* — `Name` / `FromTime` /
  `ToTime` — a name collision, not this.) Decide whether the terminal needs an
  **offline mode** (today it is online-only) while you are rebuilding it.
- Port batch/expiry → `Pharmacy` industry module (extension tables on
  Product/Stock; reuse `StockItem.BatchNo/.ExpiryDate`, `Product.IsExpiryItem`).
- POS terminal UI: port `PMSShopAngular` `modules/pos` + `pos-layout` into the
  tenant shell as a terminal mode — a contained lift (~18 components under one
  `PosComponent`; the app's `admin`/`home` modules are stubs, nothing to bring).
- Migration tooling for existing POS customers (per-DB → tenant rows).
- Retire `PMSAdminReact`. Its ~380 files / ~54 routes are mostly the POS
  back-office twin of screens the ERP shell already has — do a screen-by-screen
  overlap pass against Configuration / Accounts / Report, and rebuild only the
  POS-only ones (DamageLost, StockAdjustmentBatch, POS Sale / CustomerPayment,
  the `LegacyReports` ledgers) in Angular. Its redux is MatX boilerplate — drop
  it.

### Phase 4 — Billing automation

*Goal — money collects itself.*

- Usage metering jobs (users, branches, terminals, storage) → `UsageRecord`.
- Invoice generation on the billing cycle; proration on mid-cycle plan changes.
- Payment gateway — bKash / SSLCOMMERZ for BD, card for international.
- Dunning: PastDue → grace → read-only → suspend, with emails.
- Self-serve plan upgrade/downgrade.

### Phase 5 — Breadth

*Goal — the platform sells itself to new industries.*

- Super Shop, Wholesale, Retail, Buying House templates + any thin industry
  modules.
- Dedicated-DB option for Enterprise tenants (activate the `DbConnectionKey`
  path).
- Custom domains, white-label, tenant API keys.
- Marketplace-style module discovery inside the app.

---

## 09 · Frontend consolidation

Three frontends collapse to two apps. The tenant shell (`Application.Client`) now
sits in the same `GenericERP` repo as the platform API, so the shell and the
`/api/me` contract it depends on version and deploy together.

| App | Built from | Purpose |
|---|---|---|
| **Tenant shell** (Angular, current LTS) | `Application.Client` (was ERPAngular, already in the `GenericERP` repo), upgraded from **14.2 → current (≈4 majors)**; POS screens ported from PMSShopAngular | The whole product for a logged-in tenant: ERP modules lazy-loaded by entitlement, plus a POS terminal mode. Menu, routes and guards driven by `/api/me`. |
| **Platform console** (Angular) | New, small | Internal ops: tenants, modules, pricing, metrics, support impersonation. |
| ~~PMSAdminReact~~ (retire) | — | React 16 + MUI 4 + react-scripts 3 is a dead-end toolchain. **Bigger than "a handful of screens"** (~380 files, ~54 routes) — but most of it is the POS back-office equivalent of screens the ERP shell already has (Category, Product, Supplier, Customer, Purchase, JournalEntry…). Do a screen-by-screen overlap pass; rebuild only the genuinely POS-only ones (DamageLost, StockAdjustmentBatch, the POS Sale / CustomerPayment flows, the ledger `LegacyReports`) in the shell. |

The marketing + signup site is a separate, lighter codebase (it has no auth and
different performance/SEO needs) — it just talks to the onboarding API.

> **Menu is data, not code:** the single highest-leverage frontend change —
> delete the hardcoded `navigation.service.ts` array and build the sidenav from
> an endpoint that already knows the tenant's modules and the user's
> permissions. Everything about "configurable ERP" on the frontend follows from
> that. (The `*appHasPermission` directive already does per-item permission
> filtering; it just needs an entitlement check alongside it, and a real
> `/api/me` behind it — today there is none: `JwtAuthService.checkTokenIsValid()`
> returns a hardcoded `DEMO_USER` and token-refresh / expiry handling is
> commented out.)

> **The frontend `BusinessType` split is the bigger half of §11.** The Angular
> app decodes a `businesstype` claim and branches `businessType === '1' | '2'`
> in **~315 places across ~109 component files** — every domain — plus a
> parallel pharma-only screen tree (`views/report/components/primary-sales-module-report/`,
> `…/primary-inventory-module-report/`). Retiring this is a much larger job than
> the backend's ~32 branches, and it must land the same way: a client-side
> industry-profile service the components read, not scattered `*ngIf`. Budget it
> into the Angular upgrade, not after.

---

## 10 · The twelve questions, answered

1. **Best multi-tenancy strategy?** — **Pooled, single instance, row-level
   `TenantId` with EF global query filters.** It is where `Application.Api` already is
   and the only model that supports self-service signup and central billing.
   Harden it; don't replace it.
2. **Shared or separate databases?** — **Shared by default; dedicated DB as a
   per-tenant override for Enterprise.** Never schema-per-tenant — it multiplies
   migration cost with no isolation benefit over a discriminator column plus a
   fail-closed save guard.
3. **How should module activation work?** — **Code-defined modules, data-driven
   activation.** A seeded `Module` catalog with declared dependencies; per-tenant
   `TenantModule` rows; enforced at API (`[RequiresModule]`), navigation
   (`/api/me`), and service layer.
4. **How is subscription access enforced?** — **Middleware resolves subscription
   + entitlements into a cached `ITenantContext`;** attributes and create-time
   quota checks read it. Expiry flows Trial → Active → PastDue (grace) →
   read-only → Suspended, never an abrupt lockout.
5. **How is pricing calculated?** —
   **`planBase + Σ modulePrice + Σ (meteredQty × unitPrice)`** from a versioned
   price book; the tenant keeps their signup version until they change plan. Only
   three metered dimensions: users, branches, POS terminals. Plans are just named
   bundles over the same engine.
6. **How does business configuration work?** — **Templates + questionnaire +
   typed settings.** A `BusinessTemplate` supplies recommended modules, defaults
   and a seed pack; plain questions adjust it; the customer can override then,
   and change toggles later. No workflow invention by customers.
7. **How does it stay simple for end users?** — **Business language, not ERP
   language;** a 5-step wizard with IP-based defaults; recommendation before
   selection; a live line-item price; a getting-started checklist after signup.
   ERP vocabulary is confined to the platform console.
8. **How are new industry types added?** — **New template + seed pack + only the
   missing industry module.** Core and business modules are reused unchanged. No
   forking, no per-customer code.
9. **How is the existing Feed ERP migrated?** — **It becomes the platform** —
   and it is already multi-tenant and already two-industry (Pharmaceutical +
   Feed), so the Feed and Pharmacy templates are mostly extraction, not new
   build. Add the Platform layer, convert filtering to global query filters,
   replace `BusinessType` branches with modules + an industry profile, backfill
   current customers onto their templates. Zero rewrite; they never stop
   working.
10. **How is the Pharmacy POS reused?** — **Re-platformed into `Pos` + `Pharmacy`
    modules** (.NET 6→9, string PK→Guid, add TenantId + query filters, silo→pool,
    and adopt Autofac / `BaseService` / `UnitOfWork` — the POS has none of them).
    Domain logic worth keeping: fast sale, per-line COGS, returns, batch/expiry,
    branch transfers, customer payments. Shift/cash-drawer is new work.
    `PMSShopAngular`'s POS module/layout is the reference for the terminal UI. The
    second backend is decommissioned, not maintained in parallel.
11. **What is generic vs industry-specific?** — **Generic (all of it, for pharma
    and feed):** identity, org, product, customer, supplier, inventory, purchase,
    production, sales, POS, accounting, reporting, pricing — both industries reuse
    every existing feature. Pharma vs feed is a **behaviour profile** only (units,
    report layout, dashboard metric, one sales rule). Genuinely industry-specific
    *modules* start with new verticals: super shop (promotions), buying house
    (style costing), wholesale (routes/credit). See §05.
12. **How do we avoid over-engineering?** — **One deployable, one DB, no no-code
    engine, no per-tenant code, templates not workflow builders, manual billing
    before automated.** Ship Feed + Pharmacy first; add breadth only once those
    two pay. See §11.

---

## 11 · Risks & guardrails

| Risk | Why it bites here | Guardrail |
|---|---|---|
| **Cross-tenant data leak** | Reads: filtering is hand-written in `BaseRepository.TableNoTracking()` and fully bypassed by `TableWithoutTenant()`; one missed `.Where` exposes another tenant. Writes: `UnitOfWork` re-stamps `TenantId` on every `Modified`/`Deleted` entity without an ownership check, so an update-by-Id crosses tenants silently. | EF global query filters on `ITenantScoped`; delete `TableWithoutTenant()`; `SaveChanges` guard that compares stored vs current `TenantId`; per-endpoint two-tenant integration tests in CI. |
| **Dual-backend drift** | Running `Application.Api` and ButsPosDotnet6 in parallel indefinitely doubles every fix — and they have diverged architecturally (the POS has no Autofac, no `BaseService`, no `UnitOfWork` audit pipeline, no query filters, string PKs), so the port is a re-platform, not a merge. | Phase 3 has a hard cutover; ButsPosDotnet6 goes read-only then off. .NET 6 is already out of support — this is also a security deadline. |
| **Pricing complexity creep** | "Just one more pricing rule" is how SaaS billing becomes unmaintainable. | Three metered dimensions, flat module prices, one engine. New pricing ideas need an explicit decision, not a config toggle. |
| **Provisioning half-failures** | A tenant created without a chart of accounts is a broken tenant. | Idempotent, transactional, re-runnable provisioning keyed on `(TenantId, stepKey)`; a "provisioning failed" queue in the console. |
| **`BusinessType` branching — ~32 sites backend, ~315 across ~109 files frontend** | Backend: `if (BusinessType == Primary/Secondary)` in ~32 places + paired `…Primary…`/`…Secondary…` PDF methods. Frontend: `businessType === '1'|'2'` in ~315 places across ~109 Angular components, plus a parallel pharma-only `primary-*-module-report` screen tree — **the larger migration.** The enum members are literally named `Primary` (1, pharma) / `Secondary` (2, feed) — which **collides with the `Primary*` vs plain quantity (bags vs Kg) field concept** on every sale-detail row. A third industry as a new int value means editing every site, both tiers. | Backend: `IIndustryProfile` (`IUomPolicy`, `ISalesPolicy`, `IReportPack`); rename `BusinessType.Primary/Secondary` → `Pharmacy`/`Feed`. Frontend: a client-side industry-profile service the components read, done as part of the Angular upgrade — not scattered `*ngIf`. |
| **Scaffolded DbContext friction** | EF Core Power Tools re-scaffold overwrites hand edits; adding platform tables fights the generator. The global `Deleted` filter is re-applied via a custom `ApplyGlobalFilter` call at the end of `OnModelCreating` — a TenantId filter would need the same treatment. | Keep platform entities in a hand-authored context partial / separate configuration; document the re-scaffold procedure. |
| **Stale duplicate infrastructure project** | An orphan net6.0 `Application.Infrastructure/` folder holds an out-of-date copy of `BaseRepository.cs` / `UnitOfWork.cs` that is not in the solution; edits can land in the wrong file. | Delete it in Phase 0. The live project is `Application.Repository/Application.Infrastructure.csproj`. |
| **The temptation to go dynamic** | "Configurable" slides into "user-defined entities and forms", which is a different (much larger) product. | Written principle: *stable code + configurable features*. Configuration selects and parametrises code paths; it never defines them. |
| **Angular 14 age** | Shell is Angular 14.2 on Node 16 — roughly **four majors** behind current, and Node 16 is EOL. Upgrade cost grows monthly, and the `businesstype` de-scatter (above) rides on the same pass. Demo scaffolding (`DEMO_USER`, `publishNavigationChange`, `MATX_USER`) is still in place. | Budget the upgrade into Phase 2 before piling new screens on; fold in the `/api/me` wiring, the industry-profile service, and token-refresh (currently commented out). |
| **Seed-data divergence** | Feed and Pharmacy charts of accounts / units drift apart over time. | Seed packs are versioned artifacts owned by the platform team, not copy-pasted per template. |

---

## 12 · Requirements coverage check

Every requirement block from the brief, mapped to where this plan addresses it.
● fully addressed · ◐ addressed with a deliberate narrowing (noted).

| Requirement | | Where & how |
|---|---|---|
| SaaS product — self-service sign-up, configure, use, no per-customer dev | ● | §00 recommendation · §06 wizard · §08 roadmap Phase 2 |
| Multi-tenant · Configurable · Modular · Simple · Scalable · Subscription-based | ● | D1 (multi-tenant) · D3/D6 (modular, configurable) · §06 + language callout (simple) · §02 "How it scales" · D4/D5 (subscription) |
| Tenant = customer; supported types: Feed, Pharmacy, Super Shop, Buying House, Wholesale, Retail, Other | ● | §05 capability table · §06 template-defaults table (incl. "Other") |
| Tenant data isolated & secure | ● | D1 · D2 · §02 request-scoping flow · §11 leak guardrail |
| Setup wizard — Step 1 account, Step 2 business, Step 3 business type → recommended modules | ● | §06 five-step flow (matches the brief's steps 1:1) |
| Per-business-type recommended module lists (Pharmacy / Feed / Super Shop shown) | ● | §06 template-defaults table |
| Module configuration — Core vs Optional groups, enable/disable, auto price | ● | §06 review-step mock-up (grouped, priced live) · D3 |
| Pricing model — base + per-module + per-user + per-branch + per-POS-terminal + storage + advanced | ◐ | D5 + §06. **Narrowing:** bill 3 metered dimensions at launch (users, branches, terminals); storage & advanced-feature metering kept in the schema, switched on in Phase 4 — per the brief's own "avoid complicated pricing rules". |
| Always show what was selected and the price; simple & transparent | ● | §06 itemised quote mock-up; live update on every toggle |
| Predefined plans (Starter / Business / Enterprise) + Build-Your-Own (6 steps) | ● | §06 plans table + build-your-own 6-step list · D5 (one engine) |
| Extremely simple UX — no technical terms, business-friendly language | ● | §06 "Language discipline" callout · §10 Q7 |
| Questionnaire-based guided configuration (Q1–Q6 examples) | ● | §06 questionnaire → configuration mapping table |
| Tenant configuration architecture (Platform → Tenant → Subscription / Type / Modules / Users / Branches / Settings) | ● | §02 "Configuration lives at three levels" tree · Appendix A tables |
| Recommend architecture for: multi-tenancy, isolation, module activation, feature permissions, subscription mgmt, pricing | ● | D1 · D1/D2 · D3 · §04 + D4 · D4 · D5 |
| **Separate** module/subscription access from user roles/permissions | ● | §04 — the two-axis model, effective access = intersection |
| Separate Platform Administration area with the listed admin powers; tenant admin scoped to own business | ● | §07 — split consoles, two authorization surfaces, capability lists |
| Principle: Stable Code + Configurable Features; not a DB-driven dynamic ERP | ● | §00 · D6 · §11 "temptation to go dynamic" |
| Modular Monolith Multi-Tenant architecture (Platform / Core ERP / Business Modules / Industry Modules) | ● | §02 layer stack — matches the brief's tree |
| Future scalability — new business type = analyse → reuse Core → reuse Business → build only missing Industry module | ● | §05 "Adding a new business type later" |
| Final goal — "tell us → recommend → choose → price → start"; fast & friendly | ● | §06 opening line + flow |
| Answer the 12 specific questions | ● | §10 — all twelve |
| Practical for a real product & a small team; avoid over-engineering | ● | §08 phased delivery · §11 · §00 "what we are not doing" |
| Templates + Modules + Feature config (not arbitrary customer-built workflows initially) | ● | D6 · §00 · §11 |
| Gradually migrate the existing Feed ERP; reuse the Pharmacy POS as a source | ● | §08 Phases 0–3 · §10 Q9 & Q10 |

> **One open item for you to decide:** the only place this plan deliberately
> says "not yet" rather than "yes" is **metered storage and advanced-feature
> billing** (Phase 4). If charging for storage from day one matters to your
> business model, say so and D5 changes — the engine already supports it, it is
> only a question of when you turn it on and how you measure it.

---

## Appendix A · Platform data model sketch

New tables, all *cross-tenant* (owned by the Platform layer, not filtered by
`TenantId`). Field lists are indicative, not final.

**Cross-checked against `generic-erp-db` (2026-09-06):** of the tables below,
**only `Tenant` exists** (and `Setting`, partially — see `TenantSetting`).
Everything else is greenfield. There is no `Module`, `Subscription`, `Plan`,
`PriceBook`, `Entitlement`, `UsageRecord`, `Invoice`, `BusinessTemplate` or
`OnboardingSession` table, and nothing resembling them.

**House conventions to follow** (every one of the 110 domain tables obeys these;
the new platform tables should too, minus the tenant parts): PK is
`Id uniqueidentifier` (Guid) — no `int`/`string` keys anywhere in the domain;
audit columns are `CreatedOn` / `UpdatedOn` `datetime2` + `CreatedBy` /
`UpdatedBy` `nvarchar(200)` **(the username string, not a user Guid)** + a
`Deleted bit`. Cross-tenant platform tables keep the audit columns but are
exempt from `TenantId` stamping and the `Deleted` query filter.

| Table | Key fields | Purpose |
|---|---|---|
| `Tenant` *(exists — extend)* | *today (verified):* `Id uniqueidentifier` · `Code nvarchar(20)` · `Name nvarchar(400)` · `TimeZoneId nvarchar(max)` · `Address nvarchar(max)` · `ContactNo` · `BINNo` · `Email` · `BusinessType int` · `Logo` + audit + `Deleted` &nbsp;•&nbsp; *add:* Subdomain · CustomDomain? · BusinessTemplateKey · Status · Currency (ISO string) · DbConnectionKey? | The customer. `DbConnectionKey` null = shared pool; nothing stored today — `TenantDataContextExtension` derives the DB from the sub-domain. `BusinessType` (1 = Pharmaceutical, 2 = Feed) stays as a shim until `BusinessTemplateKey` replaces it. **No `CountryId`** in the "add" list: `Country` is a *per-tenant* lookup table (`Id`, `Name`, `TenantId`), so there is no global country master to FK — store a country code string if needed. |
| `BusinessTemplate` | Key · Name · Description · DefaultModuleKeys[] · SeedPackKey · QuestionnaireKey | Pharmacy, Feed, Super Shop, … |
| `Module` | Key · Name · Category {Core\|Business\|Industry} · DependsOn[] · MeteredBy? · PermissionGroup | The catalog. Seeded from code. |
| `TenantModule` | TenantId · ModuleKey · Status · ActivatedOn · ExpiresOn? | What this tenant has switched on. |
| `Plan` | Key · Name · IncludedModuleKeys[] · IncludedUsers · IncludedBranches · MonthlyBase · Public | Starter / Business / Enterprise bundles. |
| `PriceBook` / `PriceBookItem` | Version · Currency · EffectiveFrom • Sku · UnitPrice · Unit | Versioned prices for plans, modules, meters. |
| `Subscription` | Id · TenantId · PlanKey? · Status · PriceBookVersion · PeriodStart · PeriodEnd · TrialEndsOn | One active per tenant. `PlanKey` null = build-your-own. |
| `SubscriptionItem` | SubscriptionId · Sku · Quantity · UnitPriceSnapshot | Frozen line items = the tenant's locked price. |
| `Entitlement` | TenantId · Key {max_users, max_branches, max_pos_terminals, storage_mb} · Limit | Derived from plan + items; checked on create. |
| `TenantSetting` | TenantId · Key · Value | Typed setting store. The existing `Setting` table is `Id` · `Name` · `Value` only — **not tenant-scoped, and no audit columns** (one of only four such tables). Add `TenantId` (+ rename `Name`→`Key`) or supersede it. |
| `UsageRecord` | TenantId · Meter · Quantity · PeriodDate | Metering input for billing. |
| `TenantInvoice` / `TenantInvoiceLine` | TenantId · Number · PeriodStart/End · Status · Total • Sku · Description · Qty · Amount | Phase 4. **Do not call it `Invoice`** — `SaleInvoice` and `PurchaseInvoice` are existing, unrelated ERP documents. |
| `OnboardingSession` | Id · Email · Answers(json) · RecommendedTemplateKey · SelectedModuleKeys[] · Quote(json) · Step · ConvertedTenantId? | Pre-tenant signup state. |
| `PlatformUser` / `PlatformRole` | Id · Email · PasswordHash · Role {SuperAdmin\|Ops\|Support\|Billing} | Console identities, deliberately **separate from and unlike tenant `User`** — which is `Username` (not email) + `PasswordHash`/`PasswordSalt` `varbinary` (salted, not one string) + a `RoleId` FK to the tenant-scoped `Role` table (one role per user, not an enum). |

### Enforcement primitives (C#)

The whole gating model reduces to a small context object plus three attributes:

```csharp
public interface ITenantContext {
    Guid TenantId { get; }
    string BusinessTemplateKey { get; }            // "pharmacy" | "feed" | ...
    IReadOnlySet<string> Modules { get; }          // enabled ∩ paid
    IReadOnlyDictionary<string,int> Quotas { get; } // max_users, ...
    SubscriptionStatus Status { get; }             // Trial|Active|PastDue|...
    string? DbConnectionKey { get; }
}

public interface ITenantScoped { Guid TenantId { get; set; } }

// Replaces the scattered `if (BusinessType == Primary/Secondary)` branches.
// One implementation per industry, resolved from ITenantContext.BusinessTemplateKey.
public interface IIndustryProfile {
    string Key { get; }
    IReportPack   Reports     { get; }  // per-industry PDF/report set (Primary/Secondary packs)
    ISalesPolicy  SalesPolicy { get; }  // e.g. Feed: require approved customer-wise discount
    IDashboardSpec Dashboard  { get; }  // which KPIs the dashboard computes
    IUomPolicy    Uom         { get; }  // bags -> sellable unit
}

// Feed:   ToSellable(primaryQty, product) => primaryQty * product.BagWeight;   // Kg
// Pharma: ToSellable(primaryQty, product) => primaryQty;                       // identity
public interface IUomPolicy {
    decimal ToSellable(decimal primaryQuantity, Product product);
}

[RequiresModule(ModuleKeys.Pos)]        // 403 + "module not enabled"
[RequiresFeature("expiry_alerts")]      // finer-grained than a module
[EnforceQuota("max_branches")]          // checked before Add
```

EF wiring: in `OnModelCreating`, for every `ITenantScoped` type apply
`HasQueryFilter(e => e.TenantId == _tenantContext.TenantId)` alongside the
existing `Deleted` filter (which is applied through a custom `ApplyGlobalFilter`
helper at the end of `OnModelCreating`, not per-entity); in `SaveChanges`, throw
if any *added* `ITenantScoped` entry has `TenantId == Guid.Empty` *or* any
*modified/deleted* entry's stored `TenantId` differs from the current tenant.

**Industry profile migration:** the ~32 current `BusinessType` checks each move
to one of — a `[RequiresModule]` gate (feature only exists for that industry), a
`_tenant.Industry.SalesPolicy` call (rule differs), a `_tenant.Industry.Reports`
lookup (layout differs), or a `_tenant.Industry.Uom.ToSellable(...)` call (the
bags→Kg conversion now scattered through `SaleOrderService`,
`DeliveryNoteService`, `SaleInvoiceService`, `SaleReturnService`,
`SaleQuotationService`). The `PrimaryPermissions` / `SecondaryPermissions`
classes become the `PermissionGroup` of the Pharmacy / Feed modules, and the
`BusinessType.Primary/Secondary` enum members should be renamed `Pharmacy` /
`Feed` to end the collision with the `Primary*` quantity fields.

---

*BUTS ERP SaaS Platform Plan · v1.1 draft · 2026-09-06 · derived from review of
`GenericERP` (`Application.Api` + `Application.Client`, formerly BUTSERP_API +
ERPAngular), ButsPosDotnet6, PMSAdminReact, PMSShopAngular · figures
and field lists indicative pending detailed design.*
