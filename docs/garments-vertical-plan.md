# Adding the Garments (RMG) vertical to the ERP SaaS platform

**Enhancement plan — v0.1 draft**

How to bring an existing garments-manufacturing ERP ("SCERP") into the
`GenericERP` multi-tenant platform as a **Garments Industry** business template,
reusing everything except HR/Payroll.

| | |
|---|---|
| **Requested** | Provision a garments-manufacturing tenant; reuse all SCERP modules except HR. |
| **Source system** | `D:\MyDocuments\MyDocuments\job\Development` — `SCERP.sln` (SCERP.Model / .DAL / .BLL / .Common / .Web) |
| **Target** | `GenericERP` — .NET 9 Clean Architecture API + Angular, pooled multi-tenancy, `BusinessTemplate` + `IIndustryProfile` + `[RequiresModule]` |
| **Prepared** | 2026-09-07 · derived from the SCERP Visual Studio content index (source folders not present on disk) |
| **Sibling doc** | `docs/saas-platform-plan.md` — the platform this plugs into |
| **Interactive version** | https://claude.ai/code/artifact/afce3336-ea50-497a-8b57-1cce44aef440 |

---

## Start here — the plain version

You want a garments factory to be able to sign up for this platform and run its
whole business on it: take a buyer's order, cost it, plan the time-and-action
calendar, book yarn and fabric, knit / dye / cut / sew / finish, ship it, and
keep the books — **without** the HR and payroll side, which you already handle
elsewhere.

**The good news:** the platform is built exactly for this. Adding an industry is
"new `BusinessTemplate` row + `IIndustryProfile` + seed pack + whichever modules
fit + only the genuinely new domain logic" — no forking, no per-customer code.
Inventory, Purchase, Accounting and Reports are already here and a garments
factory uses them almost unchanged.

**The hard truth:** a garments manufacturer is the **largest vertical the
platform has taken on** — far bigger than Feed or Pharmacy. Merchandising,
Commercial (LC / export), Planning (TNA) and shop-floor Production are four
domains that **do not exist anywhere in `GenericERP` today**. SCERP has them, but
SCERP is a ~3,800-file ASP.NET MVC5 / EF6 / Crystal Reports application from
around 2015. Its modules cannot be "referenced" or "imported" — every one has to
be **re-platformed** the same way the plan re-platforms the Pharmacy POS: entities
to EF Core, business logic to `BaseService`, screens to Angular, reports to the
PDF/Excel stack. That is a multi-quarter programme, not a template tweak.

**This document** inventories what SCERP contains, maps each part to a platform
module, defines the `garments` template, and lays out a phased way to get there —
plus the two things that block starting: the SCERP source code, and a decision on
how aggressively to port vs. integrate.

---

## Contents

- [00 · What blocks starting today](#00--what-blocks-starting-today)
- [01 · What SCERP is](#01--what-scerp-is)
- [02 · Reuse map — SCERP area → platform module](#02--reuse-map--scerp-area--platform-module)
- [03 · The `garments` business template](#03--the-garments-business-template)
- [04 · "Except HR" — where the cut actually falls](#04--except-hr--where-the-cut-actually-falls)
- [05 · Porting strategy](#05--porting-strategy)
- [06 · Data & tenant strategy](#06--data--tenant-strategy)
- [07 · Phased roadmap](#07--phased-roadmap)
- [08 · Effort & sequencing](#08--effort--sequencing)
- [09 · Risks & guardrails](#09--risks--guardrails)
- [10 · What I need from you](#10--what-i-need-from-you)

---

## 00 · What blocks starting today

**1. The SCERP source is not on disk.** `D:\MyDocuments\MyDocuments\job\Development`
contains only the solution files (`SCERP.sln`, `SCERP_2015.sln`, …), the
`packages/` folder and the `.vs/` content index. The five project folders it
references — `SCERP.Model`, `SCERP.DAL`, `SCERP.BLL`, `SCERP.Common`,
`SCERP.Web` — are **missing**. Everything below is reconstructed from the Visual
Studio file-content index (file names and identifiers only), so it is a reliable
map of *what* SCERP does but tells us nothing about *how* — entity shapes, SQL,
business rules, report layouts. **Nothing can be ported until the source (and the
database / `SCERP.edmx`) is available.**

**2. A strategy decision.** SCERP is genuinely large (see §01). There are two
legitimate paths and they need your call before design starts — see §06.

---

## 01 · What SCERP is

A full **RMG (ready-made garments) manufacturing ERP** covering the whole
order-to-ship lifecycle. From the content index:

| Metric | Count (approx.) |
|---|---|
| MVC areas (modules) | 16 |
| Controllers | 426 |
| BLL "Manager" classes | 760 |
| "Repository" classes | 810 |
| ViewModels | 330 |
| Razor views (`.cshtml`) | 590 |
| Total C# files | ~3,810 |
| **Tech stack** | ASP.NET **MVC 5**, **EF 6 database-first (`SCERP.edmx`)**, Autofac.Mvc5, AutoMapper 3, **Crystal Reports**, **Handsontable 0.38** grids, Bootstrap 3, SignalR (`ProductionHub`) |

### The 16 areas

| Area | What it does | Overlap with `GenericERP` |
|---|---|---|
| **Merchandising** | Buyer, buyer order (style / colour / size), inquiry, sample development & submission, lab-dip / embellishment approval, cost sheet (templates, costing heads), consumption (fabric / yarn / thread / accessories), bulk booking, order documents | **None** — net-new domain |
| **Commercial** | Master LC / Back-to-Back LC (36 LC-related controllers), import, export, shipment, customs, cash incentive, bank advice | Partial — `Application.Api` has LC costing but not LC lifecycle / trade docs |
| **Planning** | TNA (time & action calendar), process templates & sequences, production lines, daily line layout, target production, capacity, working-day calendar | **None** — net-new |
| **Production** | Knitting (rolls), dyeing (job orders, grey/finish, re-dyeing), cutting (lay, bundle, tags, grading), sewing (input/output by process, SMV / standard-minute value), finishing, embroidery, printing, subcontract process delivery/receive | `GenericERP` has a generic "Production" (BOM + manufacturing order) — **not** shop-floor RMG process tracking. Net-new. |
| **Inventory** | Store (yarn / fabric / accessories / finish-fabric), material requisition → issue → receive, GRN against PO, gate pass, returnable challan, fabric receive / return, batch-wise issue | **High** — reuse the platform `inventory` module; add garment-specific stores & issue types |
| **Accounting** | Chart of accounts, control accounts, voucher entry (cash / bank / journal / contra), bank reconciliation, cost-centre voucher segregation, depreciation chart | **High** — reuse the platform `accounts` module; SCERP's ledger retires into it |
| **Maintenance** | Machine maintenance, breakdown, spare parts | Low — `GenericERP` has `Machine`; add maintenance module later |
| **CRM / Marketing** | Buyer client, marketing inquiry, visitor, lead | Low — thin, optional |
| **Task Management / Tracking / MIS** | Internal tasks, order tracking board, MIS & commercial reports | Low — reuse `report`; tracking board is thin |
| **User Management** | Users, roles, permissions, menu | **Replace** — the platform's auth / `RoleClaim` / `[RequiresModule]` supersedes it |
| **Common** | Countries, ports, banks, UOM, currencies, buyers' agents, seasons, brands | **High** — merge into platform `configuration` masters |
| **HRM + Payroll** | Employee master, attendance, in/out processing, salary process, penalty, bonus, OT, provident fund, gratuity, holiday, work shift/group, job card, pay slip (~57 controllers) | **EXCLUDED** — see §04 |

### The garments-specific "spine"

The parts a garments factory cannot run without and that the platform has to
grow:

```
Buyer ─▶ Inquiry ─▶ Sample (dev / submission / approval, lab-dip, embellishment)
                          │
                          ▼
        Style ─▶ Buyer Order (colour × size grid) ─▶ Cost Sheet ─▶ approval
                          │                              │
                          ▼                              ▼
        Consumption (fabric / yarn / thread / accessories per style)
                          │
                          ▼
        Bulk Booking ─▶ Yarn / Fabric Order ─▶ LC / BB-LC ─▶ import
                          │
                          ▼
        TNA calendar ─▶ Planning (line layout, target, capacity)
                          │
                          ▼
  Knitting ─▶ Dyeing ─▶ Cutting (lay / bundle / grading) ─▶ Sewing (input/output, SMV)
        ─▶ Embroidery / Print ─▶ Finishing ─▶ Finish-fabric / garment store
                          │
                          ▼
        Shipment ─▶ Export docs ─▶ cash incentive ─▶ Accounting
```

---

## 02 · Reuse map — SCERP area → platform module

The platform module catalog today: `configuration · inventory · purchase ·
sales · production · accounts · report` (all Business except `configuration`).
The garments vertical **reuses 4 of them and adds a `garments-*` family**:

| New / reused module | Key | Built from | Notes |
|---|---|---|---|
| Configuration & masters | `configuration` *(reuse)* | SCERP Common area | Add garment masters: buyer, agent, season, brand, colour, size, fabric type, yarn count, process, costing head |
| Inventory & stock | `inventory` *(reuse + extend)* | SCERP Inventory area | Add store types (yarn / grey / finish fabric / accessories), material requisition → issue → receive, returnable challan, gate pass |
| Purchase | `purchase` *(reuse)* | SCERP purchase / GRN | Yarn & accessories procurement; LC-linked purchase is a `garments-commercial` concern |
| Accounting | `accounts` *(reuse)* | SCERP Accounting area | Voucher entry, bank rec, cost-centre segregation, depreciation — folded into the platform ledger |
| Reports & analytics | `report` *(reuse)* | SCERP MIS / production / costing reports | Rebuilt on the platform PDF/Excel stack |
| **Merchandising** | `garments-merchandising` *(new)* | SCERP Merchandising area | Buyer order, style, colour/size grid, sample lifecycle, approvals |
| **Costing** | `garments-costing` *(new)* | SCERP cost-sheet / consumption | Cost-sheet templates, costing heads, per-style consumption, margin |
| **Commercial / trade** | `garments-commercial` *(new)* | SCERP Commercial area | Master LC, BB-LC, import, export, shipment, customs, cash incentive |
| **Planning (TNA)** | `garments-planning` *(new)* | SCERP Planning area | TNA calendar & templates, process sequences, line layout, target, capacity |
| **Production floor** | `garments-production` *(new)* | SCERP Production area | Knitting, dyeing, cutting, sewing, finishing, embroidery, printing, subcontract |
| Maintenance | `maintenance` *(new, later)* | SCERP Maintenance | Optional add-on module, not garments-exclusive |
| CRM | `crm` *(new, later)* | SCERP CRM / Marketing | Optional add-on |

Dependencies: the whole `garments-*` family `DependsOn = "inventory"`;
`garments-production` also `DependsOn = "garments-planning,garments-merchandising"`;
`garments-costing` `DependsOn = "garments-merchandising"`.

---

## 03 · The `garments` business template

Mirrors how `pharmacy` / `feed` are defined (`BusinessTemplateConfiguration` +
`IndustryProfiles`):

```csharp
new BusinessTemplate {
    Key = "garments",
    Name = "Garments manufacturing",
    IndustryProfileKey = "garments",
    DefaultModuleKeys = "configuration,inventory,purchase,accounts,report," +
                        "garments-merchandising,garments-costing,garments-commercial," +
                        "garments-planning,garments-production",
    IsPublic = true, SortOrder = 3,
}
```

**`GarmentsProfile : IIndustryProfile`** — what the industry actually changes in
shared behaviour:

| Member | Garments behaviour |
|---|---|
| `Uom` | Fabric in Kg / yds / metres, yarn in lbs / cones, garments in dozens / pcs — a richer `IUomPolicy` than feed's single bags↔Kg |
| `Sales` | No walk-in sales; "sale" = shipment against a buyer order; revenue recognised on shipment / export realisation |
| `Reports` | RMG report pack — cost sheet, TNA status, production efficiency (SMV), line target vs. actual, shipment / export register |
| `Dashboard` | Order book, WIP by process, on-time-delivery %, machine / line utilisation, LC exposure |

**Seed pack** (provisioning steps, on top of the existing financial-year / roles /
units / store / owner-user): garment UOMs, a default costing-head set, a default
TNA template, standard process sequence (knit → dye → cut → sew → finish →
ship), one production line, a garments chart-of-accounts overlay (WIP by process,
LC margin, cash-incentive receivable).

---

## 04 · "Except HR" — where the cut actually falls

"Reuse all modules except HR" is not a clean line, because **Production and
Planning consume HR data**:

- Sewing input/output is recorded **per line / per operator**; SMV and
  efficiency need a worker headcount.
- TNA "responsible person" and planning line layout reference employees.
- Piece-rate payroll (which you keep elsewhere) reads production output.

**The cut:**

| Keep (thin) | Drop (the ~57 HR/Payroll controllers + their tables) |
|---|---|
| A minimal **`Worker` / operator master** (name, card no, line, grade) — the platform already has `Employee`; reuse or subset it | Attendance capture, in/out processing, machine-attendance import |
| **Production line** & line-operator assignment (this is Planning, not HR) | Salary structure, salary process, pay slip, salary search |
| **Designation / grade** as lookups only | Penalty, bonus, OT calculation, attendance-bonus settings |
| | Provident fund, gratuity, loan, quit/termination, appraisal |
| | Holiday setup, work shift / work group / roster, individual holiday |
| | Employee documents / education / family / bank / address sub-records |

**Integration seam for your external HR:** publish production output
(line × style × process × qty × date) and consume a worker roster. Two endpoints,
not a module. Design them in the `garments-production` module from day one so the
piece-rate system you keep can plug in.

---

## 05 · Porting strategy

Same discipline as the plan's "re-platform, not a lift" for the POS.

| SCERP | → | `GenericERP` |
|---|---|---|
| EF 6 **database-first** (`SCERP.edmx`) | | EF Core **code-first**, entities as source of truth (`Application.Core/Entities`), hand-written configs, one migration per change — the repo already works this way since `InitialBaseline` |
| ~810 `*Repository` + ~760 `*Manager` (BLL) | | `BaseService<TEntity, …>` + `BaseRepository` + `IUnitOfWork`; one Autofac module per garments sub-domain. Most SCERP repos are thin CRUD → collapse into `BaseService`; port only the real logic (costing calc, consumption explosion, TNA date maths, SMV) |
| MVC 5 controllers returning views, 16 **Areas** | | `[ApiController]` REST controllers under `Application.Api/Controllers/Garments/`, each class gated by `[RequiresModule("garments-…")]` |
| **Crystal Reports** (`.rpt`) | | EPPlus (Excel) + iTextSharp (PDF) — the stack the platform already uses; every report is a rebuild, costed individually |
| **Handsontable** grids (colour/size matrices, cost sheets, line layout) | | Angular data-grid components in the tenant shell; the colour × size order grid and the cost sheet are the two hardest UI pieces |
| ASP.NET Identity + custom menu/permission area | | Platform JWT + `RoleClaim` + `[Authorize("Permission")]` + `/api/me`-driven menu — **do not port** SCERP's User Management |
| No `TenantId`, no soft-delete filter, no audit | | Every ported entity gets `TenantId` + `ITenantScoped` (or `ITenantSharable` for system lookups), the global query filter, and the `UnitOfWork` audit / soft-delete pipeline — non-negotiable, same as Phase 0 |
| String / int PKs, `SCERPDBContext` | | `Guid` PKs, the single pooled `DataContext` with the tenancy partial |

**Naming:** SCERP prefixes entities `OM_` (merchandising, ~48), `PROD_`
(production, ~46), `PLAN_` (planning, ~18). Drop the prefixes; namespace by
folder (`Application.Core/Entities/Garments/Merchandising/BuyerOrder.cs`).

**Shared lookups:** SCERP's countries / ports / banks / currencies / UOM merge
into the platform `configuration` masters as `ITenantSharable` system data (same
mechanism as the shared chart-of-accounts skeleton), so every garments tenant
starts with them.

---

## 06 · Data & tenant strategy

**Two legitimate paths — pick one before design starts.**

### Path A — Full port (the platform way)

Re-platform the non-HR modules into `garments-*`. Existing SCERP customers are
migrated tenant-by-tenant (their EDMX DB → platform tables, keyed on a new
`TenantId`). This is the plan's model and the only path that ends with one
codebase.

- **Pro:** one platform, self-service onboarding, per-module pricing, no
  garments silo to maintain.
- **Con:** the biggest single body of work the platform has taken on — realistically
  **12–24 engineer-months** for a credible MVP-to-parity, more for full parity.
- **MVP that is still useful:** `configuration` + `inventory` + `accounts` +
  `garments-merchandising` + `garments-costing` — a buyer/style/order/cost-sheet
  system a merchandiser can run, with stock and books, *before* the shop-floor
  modules land.

### Path B — Connected silo first (the "ButsPos initially" way)

Keep SCERP running as-is for existing garments customers. Provision a `garments`
**tenant shell** on the platform now (masters + accounting + inventory +
merchandising MVP), and **sync** the two: SCERP pushes orders / shipments /
production output to the platform via an integration endpoint; the platform is
the system of record for books and the customer-facing shell. Port modules into
the platform over time and cut SCERP over module-by-module.

- **Pro:** a garments tenant exists in weeks, not quarters; de-risks the port.
- **Con:** an integration layer and a second running system until the port
  completes.

> **Recommendation:** Path B to get a garments tenant live and learn the domain,
> with Path A running behind it. Do **not** attempt a big-bang port.

Either way the **`garments` template, `GarmentsProfile` and seed pack (§03) are
built first** — they are small, unblock provisioning a garments tenant, and are
identical work in both paths.

---

## 07 · Phased roadmap

Slots after the platform's Phase 2 (self-service onboarding). Assumes the SCERP
source is available.

### Phase G0 — Template & profile *(days)*

- `garments` `BusinessTemplate` row + `GarmentsProfile : IIndustryProfile` +
  `garments-*` module catalog entries + price-book entries.
- Garments seed pack (UOMs, costing heads, default TNA template, process
  sequence, one line, CoA overlay) added to `ProvisioningService`.
- **Done when:** the platform admin can provision a garments tenant; it logs in,
  sees the shared CoA + garment masters, runs Inventory / Purchase / Accounts.

### Phase G1 — Merchandising + costing MVP *(1–2 months)*

- Port: buyer, agent, season, brand, colour, size, style, **buyer order (colour ×
  size grid)**, order documents.
- Port: costing-head, **cost-sheet template & master**, per-style consumption
  (fabric / yarn / thread / accessories), margin.
- Angular: buyer-order grid + cost-sheet grid (the two hard grids).
- **Done when:** a merchandiser runs order-to-cost-sheet on the platform.

### Phase G2 — Commercial / trade *(1–2 months)*

- Port: Master LC / BB-LC, import, export, shipment, customs, cash incentive,
  bank advice; link to `purchase` and `accounts`.
- Shipment → revenue recognition wired into the ledger (`GarmentsProfile.Sales`).
- **Done when:** an order can be booked under LC, imported against, shipped, and
  the export proceeds + cash incentive hit the books.

### Phase G3 — Planning (TNA) *(1–2 months)*

- Port: TNA templates & calendar, process sequences, production lines, daily
  line layout, target production, capacity, working-day calendar.
- **Done when:** an order gets a TNA plan and a line assignment.

### Phase G4 — Production floor *(2–4 months)*

- Port in order of value: **cutting** (lay / bundle / tags / grading) → **sewing**
  (input / output by process, SMV) → **finishing** → knitting → dyeing →
  embroidery / print → subcontract process delivery / receive.
- WIP-by-process stock movements into `inventory`.
- The HR integration seam (§04): production-output publish + worker-roster
  consume.
- SignalR production board (SCERP has `ProductionHub`).
- **Done when:** an order can be tracked cut-to-ship on the platform.

### Phase G5 — Report packs, maintenance, CRM, retire SCERP *(ongoing)*

- Rebuild the RMG report pack (cost sheet, TNA status, SMV efficiency, line
  target/actual, shipment register, LC exposure) on EPPlus/iTextSharp.
- Optional `maintenance` and `crm` modules.
- Migrate remaining SCERP customers; decommission SCERP.

---

## 08 · Effort & sequencing

| Phase | Rough size | Blocks on |
|---|---|---|
| G0 | 3–5 days | nothing (can start now) |
| G1 | 1–2 months | SCERP source; the two Angular grids |
| G2 | 1–2 months | G1; trade-finance domain knowledge |
| G3 | 1–2 months | G1 |
| G4 | 2–4 months | G3, G1; HR integration contract |
| G5 | ongoing | G1–G4 |

Total to a **credible garments MVP** (G0–G2): ~3–5 months with the source in
hand. To **SCERP parity minus HR**: 12–24 engineer-months.

The platform's own Phases 3–5 (POS, billing, breadth) run in parallel — garments
does not block them and vice-versa, except for shared frontend-shell work
(menu-from-`/api/me`, the Angular upgrade).

---

## 09 · Risks & guardrails

| Risk | Guardrail |
|---|---|
| **No source access** — cannot port from a file index | Get `SCERP.Model/.DAL/.BLL/.Web` + the DB / `SCERP.edmx`. Nothing real starts without it. |
| **Scale underestimate** — "reuse the modules" reads as config; it is a 3,800-file port | This document. Phase it; ship G0–G2 as an MVP; Path B to de-risk. |
| **HR entanglement** in Production/Planning | The §04 cut + a designed integration seam from day one, not a retrofit. |
| **Crystal Reports** — every `.rpt` is a manual rebuild, layouts are exacting | Cost each report; rebuild only reports customers actually use; MIS "custom SQL query" screen buys time. |
| **Costing engine & consumption explosion** are the real IP — get them wrong and quotes are wrong | Port these with the original developer / a domain expert in the room; golden-file test against SCERP output. |
| **Two running systems** (Path B) drift | One system of record per fact (books + masters = platform; shop-floor = SCERP until ported); idempotent one-way sync; reconcile nightly. |
| **Trade finance (LC/BB-LC)** is regulated and bank-specific | Treat `garments-commercial` as its own hardening project; do not MVP it loosely. |
| **Tenancy debt** — SCERP has no `TenantId` / filters / audit anywhere | Every ported entity goes through the Phase 0 pipeline. No exceptions, no "port now, tenant-ise later". |

---

## 10 · What I need from you

1. **The SCERP source code** — the five project folders plus the database (a
   backup) or at least `SCERP.edmx`. Without it this plan cannot become design.
2. **Path A vs Path B** (§06) — full port, or connected-silo-first. This shapes
   everything.
3. **The HR integration contract** — what your external HR/payroll system needs
   from production output, and what worker/roster data it can give back.
4. **Priority buyers / modules** — which garments customer goes first, and which
   of merchandising / commercial / planning / production they most need. That
   sets the G1–G4 order.
5. **"Parity or MVP"** for the first release — a merchandiser's order-to-cost-sheet
   tool (G0–G2), or nothing until the shop floor is in too (G0–G4).

---

*Garments vertical plan · v0.1 draft · 2026-09-07 · reconstructed from the SCERP
Visual Studio content index — source folders were not present on disk. Figures
are indicative pending access to the SCERP source and database.*
