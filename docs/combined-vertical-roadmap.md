# One platform, every business type — the combined roadmap

**Portfolio plan — v0.1**

Rolls the two detail plans — `docs/saas-platform-plan.md` (Feed + Pharmacy + the
platform layer) and `docs/garments-vertical-plan.md` (the RMG manufacturing
vertical) — into one sequenced program covering **every** business type the
platform will serve.

| | |
|---|---|
| **Scope** | Feed · Pharmacy · Garments / RMG · Super Shop · Retail · Wholesale · Buying House |
| **Detail plans** | `docs/saas-platform-plan.md` (v1.1) · `docs/garments-vertical-plan.md` (v1.1) |
| **Prepared** | 2026-09-10 |
| **Status baseline** | Platform Phase 0–1 merged; Phase 2 partially merged (provisioning + pricing + shared CoA) |

---

## Contents

- [00 · The whole thing in one page](#00--the-whole-thing-in-one-page)
- [01 · The business types at a glance](#01--the-business-types-at-a-glance)
- [02 · The shared spine — what every vertical reuses](#02--the-shared-spine--what-every-vertical-reuses)
- [03 · The layering model (the rule that keeps this sane)](#03--the-layering-model-the-rule-that-keeps-this-sane)
- [04 · Unified module catalog](#04--unified-module-catalog)
- [05 · Per-vertical plans](#05--per-vertical-plans)
- [06 · The combined timeline](#06--the-combined-timeline)
- [07 · Sequencing logic](#07--sequencing-logic)
- [08 · Effort roll-up](#08--effort-roll-up)
- [09 · Portfolio-level risks](#09--portfolio-level-risks)
- [10 · Decisions needed](#10--decisions-needed)
- [Appendix · Cross-reference](#appendix--cross-reference)

---

## 00 · The whole thing in one page

One .NET 9 modular monolith, one Angular tenant shell, one database (pooled), one
platform-admin console. Every business type is the **same code** plus:

- a **`BusinessTemplate`** row — recommended modules + seed pack + questionnaire branch;
- an **`IIndustryProfile`** implementation — units, report pack, dashboard KPIs, a few rules;
- for the larger verticals only, a family of **`<vertical>-*` business modules**.

Two industries — **Feed** and **Pharmacy** — already run on this code and reuse
100% of the feature set; for them "industry" is a behaviour profile, never a
feature gate. Two are **thin** — Retail, Super Shop, Wholesale, Buying House
(light) — each a template + profile + at most one small module over the existing
spine. One is **large** — Garments / RMG — a ~16–26 engineer-month re-platform of
an existing 437-controller MVC5 system.

> **The program in one line:** finish the platform (Phase 2), then run a
> **Pharmacy-completion track** and a **Garments track** in parallel, dropping in
> the thin verticals wherever the spine is ready.

```
                         ┌──────────────────────────────────────────┐
   PLATFORM SPINE        │  platform layer · core ERP · business     │  ← Phase 0–1 done
   (shared by all)       │  modules · accounts + shared CoA          │    Phase 2 in progress
                         └──────────────────────────────────────────┘
                              │            │            │
             ┌────────────────┘     ┌──────┘      ┌──────┴───────────────┐
     profile-only            profile + 1 module        profile + module family
   ┌──────┬─────────┬────────┐  ┌──────────┬──────────┐   ┌──────────────────┐
   │ Feed │ Pharma  │ Retail │  │Super Shop│Wholesale │   │  Garments / RMG  │
   │ LIVE │ ERP LIVE│ Phase5 │  │ Phase 5  │ Phase 5  │   │  G0–G5, own plan │
   │      │ POS→P3  │        │  │Buying Hs.│          │   │                  │
   └──────┴─────────┴────────┘  └──────────┴──────────┘   └──────────────────┘
```

---

## 01 · The business types at a glance

| Business type | Template | Source of truth today | Disposition | Size | Status |
|---|---|---|---|---|---|
| **Feed** | `feed` (BusinessType 2) | `GenericERP` / `Application.Api` — the ERP itself | The platform *is* the Feed ERP; current customers become tenants on the `feed` template | — | **Live** |
| **Pharmacy — ERP side** | `pharmacy` (BusinessType 1) | `Application.Api` `Primary` branches | Behaviour profile — merged (`PharmacyProfile`) | — | **Live** |
| **Pharmacy — POS side** | `pharmacy` + `pos` + `pharmacy` module | ButsPosDotnet6 + PMSShopAngular + PMSAdminReact | Re-platform into `pos` + `pharmacy` modules; retire all three apps | ~40 ctrls · ~380 React files · ~18 Angular POS cmpts | **Phase 3** — not started |
| **Garments / RMG** | `garments` | SCERP (`D:\MyDocuments\MyDocuments\Development\Development`) | Re-platform minus HRM/Payroll and SCERP's `Acc_*` accounting | ~291 in-scope ctrls · ~1,050 EF6 entities · ~307 reports | **Plan v1.1** — not started |
| **Retail** | `retail` | — | Template + profile; pure reuse (inventory + purchase + sales + pos + accounts) | trivial | **Phase 5** |
| **Super Shop** | `supershop` | — (POS has only a `GifItem` line) | Template + profile + `promotions` / `barcode` over `pos` | small | **Phase 5** |
| **Wholesale** | `wholesale` | `Application.Api` territory / MO-wise collection | Template + profile + small `routes` / `credit-control` module | small | **Phase 5** |
| **Buying House (light)** | `buyinghouse` | — | Template + `style-costing` / `export-docs` over purchase + sales + accounts | small–medium | **Phase 5** (distinct from Garments — no shop floor) |

---

## 02 · The shared spine — what every vertical reuses

**Built and merged** (Phase 0–1, and the Phase 2 backend):

- **Platform layer** — `Tenant` (extended: Subdomain, BusinessTemplateKey,
  Status, Currency, DbConnectionKey), `BusinessTemplate`, `PlatformModule`
  catalog, `TenantModule`, `Plan`, `PriceBook` + `PriceBookEntry`,
  `Subscription`, `Entitlement`, `TenantSetting`, `ProvisioningStep`.
  `TenantResolutionMiddleware` → `ITenantContext` per request.
- **Platform-admin console** — `Application.PlatformConsole` (standalone Angular
  20), separate `[PlatformAuthorize]` auth surface, `PlatformAuditLog` on every
  mutation; tenants / catalog / plans / price books / usage / audit.
- **Tenancy hardening** — `ITenantScoped` global EF query filters, fail-closed
  `UnitOfWork` write guard, `ITenantSharable` shared chart-of-accounts skeleton
  (55 rows on the sentinel tenant) + shared system lookups, per-tenant unique
  codes.
- **Provisioning + pricing** — idempotent `ProvisioningService` (8 steps:
  subdomain → subscription → financial-year → units → company → default-store →
  owner-role → owner-user), `PricingEngine` (`planBase + Σ module + Σ unit`).
- **Core ERP** (always on) — identity / roles, org / branch, product / item,
  customer, supplier, settings, numbering, reporting framework.
- **Business modules** (toggled per tenant) — `inventory`, `purchase`, `sales`,
  `production`, `accounts` (+ shared CoA), `report`. All **live**.
- **Industry-profile mechanism** — `IIndustryProfile` = { `Key`, `Uom`,
  `Sales`, `Reports`, `Dashboard` }; **all backend `BusinessType` branches
  retired** (`FeedProfile` / `PharmacyProfile`).

**Not yet built — Phase 2 remainder. This is the gate for every customer-facing vertical:**

- the 5-step **signup wizard** + a lightweight marketing / signup site
- the **Angular tenant shell** — menu from `/api/me`, modules lazy-loaded by
  entitlement, subscription banners, quota prompts, self-serve module toggles
- the **Angular 14 → current upgrade** (~4 majors; Node 16 is EOL)
- the **frontend `BusinessType` de-scatter** — ~315 branch sites across ~109
  Angular components → one client-side industry-profile service
- **manual invoicing** in the platform console

---

## 03 · The layering model (the rule that keeps this sane)

```
Platform layer      cross-tenant · admin-only
  tenants · templates · module catalog · subscriptions · pricing · onboarding · billing
        │  resolves ITenantContext for every request
        ▼
Core ERP            generic · always on
  identity · org / branch · product · customer · supplier · settings · numbering · reporting
        │
        ▼
Business modules    generic · toggled per tenant
  inventory · purchase · sales · production · pos · accounting · barcode · advanced reports
        │
        ▼
Industry layer      per business type
  ├── IIndustryProfile   (units · report pack · dashboard KPIs · a few rules)   ← every vertical
  └── <vertical>-* modules  (only when a capability means nothing to the others) ← large verticals only
```

> **The hard rule, restated.** Feed and Pharmacy **reuse every feature**. An
> `IIndustryProfile` call is allowed; a `[RequiresModule("feed")]` gate on an
> existing screen is not. Industry *modules* (`garments-*`, `promotions`,
> `routes`, `style-costing`) exist only for capabilities a single vertical needs
> and that mean nothing to the others.

Three tiers of vertical:

1. **Profile-only** — Feed, Pharmacy (ERP side), Retail. Template +
   `IIndustryProfile`, zero new modules.
2. **Profile + one thin module** — Super Shop, Wholesale, Buying House (light),
   Pharmacy (POS side). A handful of entities / services / screens + a report pack.
3. **Profile + a module family** — Garments. Six `garments-*` modules, four
   net-new domains, its own phased sub-plan.

---

## 04 · Unified module catalog

Every module across every vertical, in one place.

| Module | Category | Enabled for | Status |
|---|---|---|---|
| `configuration` | Core | all | **live** |
| `inventory` | Business | all | **live** |
| `purchase` | Business | all | **live** |
| `sales` | Business | feed · pharmacy · retail · supershop · wholesale · buyinghouse | **live** |
| `production` | Business | feed · pharmacy | **live** |
| `accounts` (+ shared CoA) | Business | all | **live** |
| `report` | Business | all | **live** |
| `pos` | Business | pharmacy · retail · supershop | **Phase 3** |
| `barcode` / labels | Business (thin) | pharmacy · supershop | Phase 3 / 5 |
| `notifications` | Cross-cutting | garments (then any) | **Garments G0** |
| document-attachments | Cross-cutting service | garments (then any) | **Garments G0** |
| `maintenance` | Cross-cutting | garments | Garments G5 |
| `crm` | Cross-cutting | garments | Garments G5 |
| `tasks` | Cross-cutting | garments | Garments G5 |
| `gate` | Cross-cutting | garments | Garments G5 |
| `pharmacy` (batch · expiry · drug schedule) | Industry | pharmacy | **Phase 3** |
| `promotions` / shelf / weigh-scale | Industry | supershop | Phase 5 |
| `routes` / van-sales / `credit-control` | Industry | wholesale | Phase 5 |
| `style-costing` / `export-docs` | Industry | buyinghouse | Phase 5 |
| `garments-merchandising` | Industry | garments | **Garments G1** |
| `garments-costing` | Industry | garments | **Garments G1** |
| `garments-commercial` | Industry | garments | **Garments G2** |
| `garments-planning` | Industry | garments | **Garments G3** |
| `garments-production` | Industry | garments | **Garments G4** |
| `garments-quality` (assembled) | Industry | garments | **Garments G4** |

**Industry profiles:** `FeedProfile` ✅ · `PharmacyProfile` ✅ · `GarmentsProfile`
(G0) · `RetailProfile` / `SuperShopProfile` / `WholesaleProfile` /
`BuyingHouseProfile` (Phase 5).

> Garments deliberately does **not** enable `sales` or `production` — buyer
> orders live in `garments-merchandising` and the shop floor in
> `garments-production`. It **does** use `accounts` unchanged (SCERP's `Acc_*` is
> dropped; garments posts through four adapter seams — see the garments plan §02).

---

## 05 · Per-vertical plans

### Feed — done; it is the reference

The platform *is* the Feed ERP. The only remaining work is the shared Phase 2:
put current Feed customers on the `feed` template (they already carry
BusinessType 2) behind the wizard and the new shell. No vertical-specific build.

### Pharmacy — half done

- **ERP side: live.** `PharmacyProfile` merged; `Primary` branches retired —
  A5 layouts, expiry-emphasis dashboards, product labels with pack size all run
  off the profile.
- **POS side: Platform Phase 3.**
  - Re-platform `ButsPosDotnet6` → `pos` business module: .NET 6 → 9, string PK →
    `Guid`, add `TenantId` + the global query filters, silo → pool, and adopt the
    patterns it lacks (Autofac per-domain module, `BaseService`, the `UnitOfWork`
    audit / soft-delete pipeline).
  - **Design the till-session / shift / cash-drawer model** — it exists in none
    of the three source apps (the ERP's `Shift` table is employee scheduling, a
    name collision).
  - Port batch / expiry → `pharmacy` industry module (extension tables; reuse
    `StockItem.BatchNo` / `.ExpiryDate`, `Product.IsExpiryItem`).
  - Port `PMSShopAngular` `modules/pos` + `pos-layout` → terminal mode in the shell.
  - Screen-by-screen overlap pass on `PMSAdminReact` (~380 files, ~54 routes);
    rebuild only the POS-only screens (DamageLost, StockAdjustmentBatch, POS
    Sale / CustomerPayment, the `LegacyReports` ledgers) in Angular.
  - Migrate each pharmacy DB → one tenant. **Retire all three apps.**
- **Pharmacy template GA** at the end of Phase 3.

### Garments / RMG — the large vertical

Own sub-plan: **`docs/garments-vertical-plan.md` v1.1**. Six phases, ~16–26
engineer-months to in-scope parity:

| Phase | ~Effort | Delivers |
|---|---|---|
| **G0** | ~1 mo | `garments` template + `GarmentsProfile` + seed pack; extend `configuration` / `inventory`; build `notifications` + document-attachments. **No SCERP accounting code** — use the Feed-ERP `accounts` module + a garments CoA overlay + 4 adapter seams (buyer payment → AR, export proceeds → AR, cash incentive → other income, LC / import / subcontract → AP). |
| **G1** | 2–3 mo | Merchandising + costing MVP — buyer order (colour × size grid), the approval workflows on one generic engine, the cost sheet (its own cost-group model, **not** the accounting cost centre). |
| **G2** | 2–3 mo | Commercial / trade finance — LC, BB-LC, import, export, shipment, packing credit, cash incentive. |
| **G3** | 1–2 mo | Planning — TNA, process sequences, line layout, target / capacity, operator skill matrix (moved out of the HRM model). |
| **G4** | 3–5 mo | Production floor — cutting (+ the `SpCuttingJobCard`, distinct from HR) → sewing (SMV) → finishing → knitting → dyeing → subcontract; quality assembled from QC certificate + spec-sheet + reject/grading; the HR integration seam. **The longest single item in the program.** |
| **G5** | 2–4 mo | Add-ons (`maintenance` / `crm` / `tasks` / `gate`), the ~307-report rebuild, migrate remaining customers, decommission SCERP. |

**Excluded:** HRM / Payroll (external HR + a production-output / roster
integration seam) and SCERP's `Acc_*` accounting.

### Retail / Super Shop / Wholesale / Buying House (light) — thin, Phase 5

Each is a `BusinessTemplate` row + seed pack + questionnaire branch + an
`IIndustryProfile` + at most one small module + price-book entries.

- **Retail** — pure reuse, profile only. Days.
- **Super Shop** — needs `pos` (Phase 3) + a `promotions` module (combo / gift /
  discount rules — the POS has only a `GifItem` line today) + `barcode`.
  ~1–2 engineer-months.
- **Wholesale** — `routes` / van-sales / `credit-control` over `sales`; territory
  and MO-wise collection already exist. ~1–2 engineer-months.
- **Buying House (light)** — `style-costing` + `export-docs` over `purchase` +
  `sales` + `accounts`; the non-manufacturing cousin of Garments. ~2
  engineer-months. If a customer needs the shop floor, that is Garments, not this.

---

## 06 · The combined timeline

Relative waves; calendar depends on team size. Assumes **one platform / pharmacy
squad** plus (from W2) a **dedicated Garments squad**.

| Wave | Platform / Pharmacy track | Garments track | Thin verticals | Exit criterion |
|---|---|---|---|---|
| **W1** | Finish Phase 2 — wizard, tenant shell, Angular upgrade + frontend de-scatter, manual invoicing | **G0** in parallel (template, seed pack, notifications — no wizard dependency) | — | A stranger signs up for a **Feed** ERP unaided → **Feed GA** |
| **W2** | **Phase 3** starts — `pos` re-platform, till-session model, terminal UI | **G1** — merchandising + costing MVP | **Retail** template (days) | Garments cost-sheet demo on real data |
| **W3** | Phase 3 finishes → **Pharmacy GA**; retire ButsPos / PMSAdmin / PMSShop. **Phase 4** (billing automation) starts | **G2** — commercial / LC | **Wholesale** template | Pharmacy self-service; billing runs unattended |
| **W4** | Phase 4 finishes; Phase 5 polish — custom domains, white-label, tenant API keys | **G3** — planning / TNA | **Super Shop** (needs W2 `pos`) | — |
| **W5** | — | **G4** — production floor (the long pole) | **Buying House** (light) | A garments order tracked cut-to-ship |
| **W6** | — | **G5** — reports + add-ons; migrate + retire SCERP | — | SCERP decommissioned; **all seven business types GA** |

---

## 07 · Sequencing logic

1. **Phase 2 first, always.** No vertical ships to a customer without the wizard
   + tenant shell + the Angular upgrade. The **frontend `BusinessType`
   de-scatter is on this critical path** — it must land before Garments piles
   ~250 new screens onto the shell.
2. **Garments G0 can start immediately** — backend template / seed / notifications
   work with no dependency on the wizard, and it unblocks garments provisioning.
3. **Pharmacy Phase 3 and Garments G1–G4 run in parallel** — different squads, no
   shared code beyond the spine. The one shared touch-point is the tenant shell;
   fix the module-lazy-load contract once, early.
4. **Super Shop waits for `pos`** (Phase 3). Retail / Wholesale / Buying House do
   not — slot them wherever there is capacity.
5. **Billing automation (Phase 4) is not a launch blocker** — manual invoicing
   carries Feed and Pharmacy GA. Slot it when the platform squad frees up after
   Phase 2 / Phase 3.
6. **Garments G4 is the single longest item in the whole program** (3–5 months).
   Everything else should be done or de-risked before the team is deep in it.

---

## 08 · Effort roll-up

| Item | Engineer-months | Notes |
|---|---:|---|
| Platform Phase 0–1 | — | done |
| Platform Phase 2 remainder | ~3–5 | mostly the Angular upgrade + the ~315-site frontend de-scatter |
| Platform Phase 3 — Pharmacy POS | ~4–6 | re-platform + till-session model + 3-app retirement + data migration |
| Platform Phase 4 — billing | ~2–3 | metering, invoices, gateway, dunning |
| Platform Phase 5 — polish | ~1–2 | custom domains, white-label, API keys |
| **Garments G0–G5** | **~16–26** | own sub-plan; dominates the program |
| Retail template | ~0.25 | pure reuse |
| Super Shop | ~1–2 | promotions + barcode over `pos` |
| Wholesale | ~1–2 | routes + credit control |
| Buying House (light) | ~2 | style costing + export docs |
| **Total remaining** | **~31–50** | ~55–70% is Garments |

Calendar: with two squads (platform / pharmacy, and garments) the whole portfolio
is roughly a **two-year program** — Feed GA in the first quarter, Pharmacy GA
around the halfway mark, Garments and the thin verticals filling the back half.
One squad serially pushes it toward **three to four years**.

---

## 09 · Portfolio-level risks

| Risk | Guardrail |
|---|---|
| **Frontend de-scatter slips** and Garments starts adding screens to an un-upgraded, `*ngIf`-scattered shell | Make the Angular upgrade + the industry-profile service a hard gate on Garments G1 — no `garments-*` screen merges until the shell is current |
| **Garments dwarfs everything and starves the other tracks** | Separate squad, separate branch cadence; the platform / pharmacy track must reach Pharmacy GA *before* the garments team hits G4 |
| **Two POS-terminal designs** (Super Shop vs Pharmacy) diverge | One `pos` module, one terminal mode; Super Shop is `promotions` layered on it, never a fork |
| **Accounting pulled four ways** (Feed, Pharma light-ledger retirement, Garments adapters, thin verticals) | The shared `accounts` module + shared CoA skeleton is the single ledger; every vertical posts through documented adapter seams, never a second books implementation |
| **Seed-pack drift** across seven templates | Versioned seed-pack artifacts owned by the platform team; templates compose them, never copy-paste |
| **`BusinessType` enum collision** — members are literally `Primary` / `Secondary`, colliding with the `Primary*` (bags) vs plain (Kg) quantity fields; worsens as more verticals get int values | Rename the enum members to `Pharmacy` / `Feed`; new verticals key off `BusinessTemplateKey` (string), not the int |
| **Test suite can't run** — Windows Application Control blocks a freshly-built `Application.Tests.dll` (0x800711C7) | Environment fix — allow the DLL in Smart App Control / WDAC; unblock before Phase 3, where the POS re-platform needs its regression net |
| **Marketing / signup site scope creep** | It has no auth and different SEO / performance needs — keep it a separate, lighter codebase that only talks to the onboarding API |

---

## 10 · Decisions needed

Consolidated from both sub-plans plus the portfolio view.

**Platform**

1. Metered **storage & advanced-feature billing** from day one, or defer to Phase 4? (`saas-platform-plan` §12)
2. POS terminal — **offline mode**, or online-only like today? (Phase 3)

**Garments** (`garments-vertical-plan` §11)

3. **Path A** (full port) vs **Path B** (connected-silo-first, recommended)
4. The **HR integration contract** with the external payroll
5. **Piece-rate** — sewing output stays, salary calc goes: confirm the split
6. `LoanGiven` / `LoanReturn` — employee loan (drop) or inter-party material loan (keep)?
7. **Priority garments customer** + which modules they need first (sets the G1–G4 order)
8. **Parity vs MVP** for the first garments release
9. The **SCERP database backup** (for the `CompId` → `TenantId` migration + golden-file costing tests)

**Portfolio**

10. **Team shape** — one squad serially, or a platform / pharmacy squad + a garments squad in parallel? (Two-year vs three-to-four-year program.)
11. Is **Buying House (light)** actually wanted, or is Garments the only RMG play? (Affects Phase 5 scope.)

---

## Appendix · Cross-reference

| For detail on | See |
|---|---|
| Platform layer, tenancy, module gating, onboarding, pricing, the Pharmacy POS | `docs/saas-platform-plan.md` (v1.1) |
| Garments G0–G5, SCERP coverage, the accounting adapters | `docs/garments-vertical-plan.md` (v1.1) |
| Phase 0–2 build status, branch / commit map | `phase1-platform-progress` memory + `CLAUDE.md` |

---

*Combined vertical roadmap · v0.1 · 2026-09-10 · a portfolio view over
`saas-platform-plan.md` (v1.1) and `garments-vertical-plan.md` (v1.1). Effort
figures are indicative and roll up the two sub-plans; calendar depends on team
shape (decision 10).*
