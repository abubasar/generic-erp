# Adding the Garments (RMG) vertical to the ERP SaaS platform

**Enhancement plan — v0.2 draft** *(v0.2: full area-by-area coverage check; skill matrix / org tree / calendar moved out of the HR cut; quality, notifications, documents, tasks, gate, maintenance, CRM added as modules)*

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
domains that **do not exist anywhere in `GenericERP` today** — plus quality,
templated notifications, document attachments and an ad-hoc report builder the
platform also lacks. SCERP has all of it, but SCERP is a ~3,800-file / ~348
in-scope-controller ASP.NET MVC5 / EF6 / Crystal Reports application from around
2015. Its modules cannot be "referenced" or "imported" — every one is
**re-platformed** the same way the plan re-platforms the Pharmacy POS: entities
to EF Core, business logic to `BaseService`, screens to Angular, reports to the
PDF/Excel stack, every entity through the Phase-0 tenancy pipeline. "Reuse every
feature except HR" is real and this plan covers all of it (see the Appendix) —
but it is an **18–30 engineer-month programme**, not a template tweak.

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
| MVC areas | 16 (+ 2 stand-alone services: SMS, email) |
| Controllers | 426 total — **~348 in scope**, ~78 HR/Payroll excluded |
| BLL "Manager" classes | ~760 |
| "Repository" classes | ~810 |
| ViewModels | ~330 |
| Razor views (`.cshtml`) | ~590 |
| Total C# files | ~3,810 |
| **Tech stack** | ASP.NET **MVC 5**, **EF 6 database-first (`SCERP.edmx`)**, Autofac.Mvc5, AutoMapper 3, **Crystal Reports**, **Handsontable 0.38** grids, Bootstrap 3, SignalR (`ProductionHub`), a **mobile client** (consumes `MobileAppsReport`) |

### The 16 areas — full inventory

Every non-HR area is in scope. Controller counts are from the VS index and
overlap (some controllers touch two areas); they size effort, not a shopping list.

| Area | Ctrls | What it does — the complete feature set | Disposition |
|---|---:|---|---|
| **Merchandising** | ~86 | Buyer / buyer client / contact / agent / consignee / party; inquiry; **style** (colour / size grid); **buyer order** (colour × size); order type / season / brand / category; **sample** (development → submission → approval, by size & colour) + sample type & documents; **lab dip** (development → submission → approval → options → documents); **embellishment** (type → development → submission → approval → documents); **trims & accessories** (type → development → submission → approval → history → documents); **spec sheet**; **cost sheet** (template, master, costing head, cost definition, multi-layer cost centre); **consumption** (fabric / yarn / thread / component, per style, with cost); style payment; order document | **New** — `garments-merchandising` + `garments-costing` |
| **Commercial** | ~24 | Master **LC**, **Back-to-Back LC** (+ BB-LC purchase, cash LC, cash BB-LC, cash-LC dyes/chemical), LC-order / LC-style; **import** + import details; **export**; **shipment** + style shipment + buy-order-shipment; **packing credit**, packing / package / shipping information; port of loading; **cash incentive**; stamp amount; bank advice; LC/BB-LC info data | **New** — `garments-commercial` |
| **Planning** | ~29 | **TNA** (time & action calendar, horizontal view, template, group update, responsible person), **process** (sequence, template, sequence default, group sub-process, key process, sub-process), **production line** & daily line layout, **target production**, **capacity**, **efficiency rate**, working-day calendar, knitting / collar-cuff / yarn-dyeing **programs**, fabric sub-process delivery/receive challan | **New** — `garments-planning` |
| **Production** | ~69 | **Knitting** (batch, roll, roll issue, machine, processor, program, order program, collar-cuff program/receive, grey delivery gatepass, grey issue/register, general yarn delivery); **Dyeing** (job order, SP challan, dyeing factory, dyes/chemical register, yarn-dyeing program, re-dyeing receive/issue, fabric register, fabric manufacturing cost); **Cutting** (cutting, cutting sequence, cutting tag, cut bank, cut-fabric reject, lay / roll / part / bundle cutting, grading, cutting-process style-active); **Sewing** (sewing, input process, output process, standard-minute value, key process, group sub-process); **Finishing** (finishing, iron finishing, poly finishing); **Embroidery** (process, receive); **Printing** (process, print receive); **Subcontract** (fab sub-process delivery/receive); reject adjustment, machine interruption, non-productive time; **batch / batch-roll / lot** tracking; SignalR production board | **New** — `garments-production` |
| **Quality** | ~4+ | **Quality certificate** + detail, **specification sheet**, in-line quality — cutting grading, cut-fabric reject, sewing reject, reject adjustment; AQL / defect capture at each process | **New** — `garments-quality` (some checks live inside `garments-production`) |
| **Inventory** | ~76 | **Stores** — yarn, grey fabric, finish fabric, accessories, housekeeping/consumables, item store / type / mode; **item master** (inventory item, yarn count, fabric type); **material** requisition → issue → receive (general / advance / batch-wise / accessories / collar-cuff / fabric / yarn); **GRN** / goods-receiving-note / receive-against-PO; **returnable challan** (issue / receive / master); fabric & yarn return; store purchase & requisition; daily fabric receive; **booking** (bulk / yarn / accessories); inventory approval status & authorised person | **Reuse + extend** — `inventory` |
| **Accounting** | ~26 | Chart of accounts, **control accounts** + reparent (GL head group change / by parent, control change by parent, GL account hidden); **voucher entry** — cash / bank / journal / contra / common + voucher list & segregation to cost centre; **cost centre** (single + multi-layer); **opening balance**; **financial period / year**; **bank reconciliation** + list; **depreciation chart**; **multi-currency** (AccCurrency, currency common, currency-common vouchers); advanced income tax; bank account type | **Reuse + extend** — `accounts` (opening balance, GL reparent, multi-currency vouchers, AIT are additions) |
| **Maintenance** | ~10 | **Machine** master + action + log, **machine interruption**, **non-productive time**, down-time category, **maintenance report**; **vehicle** master + vehicle gate entry | **New** — `maintenance` (add-on module) |
| **CRM / Marketing** | ~9 | Marketing inquiry, marketing institute, sales contact, buyer client, feedback, marketing reports, visitor report | **New** — `crm` (add-on module) |
| **Task Management** | ~8 | Task + status + type, assignee, follow-up, subject, **notification board** + recipients | **New** — `tasks` (add-on module) |
| **Tracking** | ~7 | **Order tracking board** — order information, ready status, sending status, confirmation media, **process-status auto-mail**, approval status | **New** — folds into `garments-merchandising` (order board) + notifications |
| **MIS** | ~25 | MIS dashboard, MIS report, MIS commercial report, **mobile-apps report** (a mobile client exists), **custom report** + **custom SQL query** (ad-hoc report builder), user report, style-costing report, production report, maintenance report, visitor report, report image | **Reuse + extend** — `report` (dashboards, the ad-hoc SQL builder, mobile-report API) |
| **Common** | ~36 | **Geography** — country → state → city → district → police station, port of loading; **org tree** — company / companies / company sector / active company sector / company organogram / factory / dyeing factory / branch / branch unit / branch-unit-department / department / department-line / department-section / unit / unit-department / section / lines / head of department; **lookups** — measurement unit, unit, payment term(s), order type, yarn count, fabric type, generic name, colour, size, supplier company, party | **Merge** — `configuration` (shared system data + tenant masters) |
| **Common — Email/SMS** | +2 solutions | **`SCERP.Message.Service`** (SMS gateway worker) + **`SCERP_2015_EmailService`** (email worker); email template + template-user + email user, mail send, process-status mail | **New** — `notifications` (templated email/SMS + event triggers; platform has raw `MailService`/`SmsService` only) |
| **Document management** | ~10 | `Document`, common file upload, order / sample / lab-dip / embellishment / trims documents, project document info, report image, sticker/label | **New** — cross-cutting attachment service (platform has `Picture` only) |
| **Security / Gate** | ~4 | **Gate pass**, **vehicle gate entry**, **visitor gate entry**, grey-delivery gatepass | **New** — `gate` (thin add-on) |
| **User Management** | ~20 | Users, user role, user activity, department- / employee-level permissions, user–merchandiser & user–TNA-responsible mapping, authorization type, authorised person, modules / module feature, entitlements, menu tree | **Replace** — platform JWT + `RoleClaim` + `[RequiresModule]` + `/api/me`; keep only the user–merchandiser / user–line mapping concept |
| **HRM + Payroll** | ~78 | Employee master + 12 sub-records; attendance (daily, in/out, manual, machine import, OT); **leave** (self / other / paper / approval / recommendation / types / settings / maternity / short / outstation / exception day); **salary** (setup / mapping / process / search / increment / advance / grade-% / excluded); bonus, attendance bonus, penalty, OT (settings / eligible / line hours); provident fund, gratuity, loan; job-card salary processing; holiday setup; work shift / roster / group; quit type; HR reporting hierarchy | **EXCLUDED** — see §04. **Skill matrix moves to Planning; a thin worker/line master and the org tree are kept.** |

### The garments-specific "spine"

The parts a garments factory cannot run without and that the platform has to
grow:

```
Buyer ─▶ Inquiry ─▶ Sample dev → submission → approval
                     (+ lab dip · embellishment · trims & accessories · spec sheet — each: dev → submit → approve)
                          │
                          ▼
        Style ─▶ Buyer Order (colour × size grid) ─▶ Cost Sheet (template · costing heads) ─▶ approval
                          │                              │
                          ▼                              ▼
        Consumption (fabric / yarn / thread / component, per style, with cost)
                          │
                          ▼
        Bulk / Yarn / Accessories Booking ─▶ Yarn / Fabric Order ─▶ LC / BB-LC ─▶ import
                          │
                          ▼
        TNA calendar ─▶ Planning (process sequence · line layout · target · capacity · efficiency)
                          │
                          ▼
  Knitting ─▶ Dyeing ─▶ Cutting (lay / roll / bundle / tag / grading) ─▶ Sewing (input/output, SMV)
        ─▶ Embroidery / Print ─▶ Finishing (iron / poly) ─▶ Finish-fabric / garment store
        │        │
        │        └─▶ Quality (certificate · spec sheet · reject/AQL at each process)
        └─▶ Subcontract process delivery / receive
                          │
                          ▼
        Shipment ─▶ Export docs / packing credit ─▶ cash incentive ─▶ Accounting
              │
              └─▶ Order tracking board · process-status auto-mail  ·  Documents attach at every step
```

---

## 02 · Reuse map — SCERP area → platform module

Platform catalog today: `configuration · inventory · purchase · sales ·
production · accounts · report`. The garments vertical **reuses 4, extends 2, and
adds a `garments-*` family + 4 cross-cutting add-ons** — nothing from SCERP is
dropped except HR:

| Module | Key | Source | Disposition |
|---|---|---|---|
| Configuration & masters | `configuration` | SCERP Common (geography tree, org tree, lookups) | **Reuse + extend** |
| Inventory & stock | `inventory` | SCERP Inventory (stores, requisition/issue/receive, GRN, returnable challan, booking, housekeeping) | **Reuse + extend** |
| Purchase | `purchase` | SCERP store purchase / GRN | **Reuse** |
| Accounting | `accounts` | SCERP Accounting (+ opening balance, GL reparent, multi-currency vouchers, AIT, depreciation) | **Reuse + extend** |
| Reports & analytics | `report` | SCERP MIS (dashboards, ad-hoc SQL builder, mobile-report API, all report packs) | **Reuse + extend** |
| Merchandising | `garments-merchandising` | SCERP Merchandising + Tracking (buyer order colour×size, style, all sample / lab-dip / embellishment / trims approval workflows, spec sheet, order tracking board) | **New** |
| Costing | `garments-costing` | SCERP cost sheet + consumption (templates, costing heads, cost definition, per-style consumption & cost, margin) | **New** |
| Commercial / trade | `garments-commercial` | SCERP Commercial (Master LC, BB-LC, cash LC, import, export, shipment, packing credit, cash incentive, port of loading) | **New** |
| Planning | `garments-planning` | SCERP Planning + skill matrix (TNA, process sequence/template, line layout, target, capacity, efficiency, programs, working-day calendar, **operator skill matrix**) | **New** |
| Production floor | `garments-production` | SCERP Production (knitting, dyeing, cutting, sewing/SMV, finishing, embroidery, printing, subcontract, batch/roll/lot, reject, machine interruption) | **New** |
| Quality | `garments-quality` | SCERP Quality (certificate, spec sheet, AQL/reject at each process) | **New** |
| Maintenance | `maintenance` | SCERP Maintenance (machine action/log, interruption, down-time, vehicle) | **New add-on** |
| CRM & marketing | `crm` | SCERP CRM/Marketing (marketing inquiry, institute, sales contact, feedback) | **New add-on** |
| Tasks & notifications | `tasks` + `notifications` | SCERP Task Management + the SMS & Email service projects (task board, templated email/SMS, process-status auto-mail, notification board) | **New add-on** |
| Gate & security | `gate` | SCERP gate pass / vehicle gate / visitor gate | **New add-on (thin)** |
| Document attachments | *(cross-cutting service)* | SCERP Document / file upload / order-sample-labdip-trims documents | **New** — a tenant-scoped attachment store used by every module |

Dependencies: `garments-*` all `DependsOn = "inventory"`; `garments-costing` on
`garments-merchandising`; `garments-planning` on `garments-merchandising`;
`garments-production` on `garments-planning,garments-merchandising`;
`garments-quality` on `garments-production`; `garments-commercial` on
`garments-merchandising,purchase`.

**Replaced, not ported:** SCERP's User Management area (users / roles /
permissions / menu / module-feature) — the platform's JWT + `RoleClaim` +
`[RequiresModule]` + `/api/me`-driven menu already does this and does it
multi-tenant. Keep only the *user → merchandiser* and *user → line* mapping
concept as data.

---

## 03 · The `garments` business template

Mirrors how `pharmacy` / `feed` are defined (`BusinessTemplateConfiguration` +
`IndustryProfiles`):

```csharp
new BusinessTemplate {
    Key = "garments",
    Name = "Garments manufacturing",
    IndustryProfileKey = "garments",
    DefaultModuleKeys =
        "configuration,inventory,purchase,accounts,report," +
        "garments-merchandising,garments-costing,garments-commercial," +
        "garments-planning,garments-production,garments-quality," +
        "maintenance,crm,tasks,notifications,gate",
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

The HRM + Payroll areas are **~78 controllers**. But those areas also hold three
things that Planning and Production genuinely need, and that are **not payroll** —
so they move out of the excluded set rather than being lost:

| Moves out of "HR" — kept | Goes to |
|---|---|
| **Operator skill matrix** — skill sets, skill operation, skill-set category / difficulty, skill-matrix grade / process | `garments-planning` — line balancing and process assignment depend on it |
| **Org tree** — company / branch / branch-unit / department / department-line / department-section / section / line / unit | `configuration` — production, inventory stores and accounting cost centres all reference it |
| **Working-day / holiday calendar** (the calendar, not leave admin) | `garments-planning` — TNA date arithmetic needs it |
| A thin **worker / operator master** — name, card no, line, grade, skill — a subset of the platform's existing `Employee` | `configuration` |

**Genuinely dropped** (kept in your external HR system):

- Employee master's 12 sub-records — address, appointment, bank, company info, documents, education, entitlement, family, follow-up, job type, personal skill, type
- Attendance — daily, in/out edit & process, manual, manual OT, machine-attendance import, job card (attendance side)
- Leave — self / other-staff / paper-based / approval / recommendation / types / settings / maternity / short / outstation duty / exception day / general-day setup
- Salary — setup / mapping / process / search / increment letter / advance / grade-% / excluded-from-process
- Compensation — bonus, attendance bonus + settings, penalty + type, OT settings / eligible / line hours, provident fund, gratuity, loan given / return, salary advance
- Roster — work shift / roster / group, holiday setup, individual holiday, branch-unit work shift
- HR reporting hierarchy (organogram used for leave approval), quit type, appraisal, employee card print

**One decision for you:** *piece-rate.* Sewing output is captured per line / per
operator (`SewingOutputProcess` — kept, it is production data). The **salary
calculation** off that output is dropped. Your external payroll reads the output.
Confirm that split, and whether **loan given / return** is an employee-loan (HR,
drop) or an inter-party loan (accounts, keep).

**Integration seam** — built into `garments-production` from day one, not
retrofitted:

- `POST /garments/production-output` consumed by nothing internally — a **webhook /
  export** your payroll polls: line × operator × style × process × qty × date × SMV.
- `PUT /garments/worker-roster` — your HR pushes the active worker list (id, name,
  card, line, grade, skill, active-from/to) so Planning always has a current
  headcount without owning HR.
- Leave affects capacity: your HR also pushes **daily availability** (line × date ×
  present count) or the planning capacity numbers are entered manually.

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

### Phase G0 — Template, profile, masters, extend the reused modules *(≈1 month)*

- `garments` `BusinessTemplate` + `GarmentsProfile : IIndustryProfile` +
  `garments-*` / add-on catalog entries + price-book entries.
- Garments seed pack (UOMs, costing heads, default TNA template, process
  sequence, one line, CoA overlay) in `ProvisioningService`.
- **`configuration` extend:** geography tree, org tree (company → branch → unit →
  department → section → line), garment lookups (yarn count, fabric type,
  generic name, payment terms, party, supplier company), thin worker master.
- **`accounts` extend:** opening balance, GL head reparent, multi-currency
  vouchers, advanced income tax, depreciation chart.
- **`inventory` extend:** garment store types, requisition → issue → receive
  variants, returnable challan, booking, housekeeping/consumables store.
- **Cross-cutting:** the tenant-scoped **document attachment** service; the
  `notifications` module (email/SMS templates + event triggers).
- **Done when:** a garments tenant is provisioned, logs in, runs Inventory /
  Purchase / Accounts with garment masters and attachments.

### Phase G1 — Merchandising + costing MVP *(2–3 months)*

- Buyer / agent / consignee / party, style (colour × size), **buyer order
  (colour × size grid)**, order type / season / brand.
- **All approval workflows** — sample (dev → submission → approval by size &
  colour), lab dip (+ options + documents), embellishment, **trims & accessories**
  (+ history), spec sheet.
- **Costing** — costing head, cost definition, cost-sheet template & master,
  multi-layer cost centre, per-style consumption (fabric / yarn / thread /
  component) with cost, margin.
- **Order tracking board** — order info, ready / sending status, process-status
  auto-mail (uses `notifications`).
- Angular: the buyer-order grid and the cost-sheet grid (the two hard grids).
- **Done when:** a merchandiser runs inquiry → sample approval → order →
  cost-sheet on the platform, with documents and status mail.

### Phase G2 — Commercial / trade *(2–3 months)*

- Master LC, **BB-LC** (+ purchase, cash LC, cash BB-LC, cash-LC dyes/chemical),
  LC-order / LC-style; import + details; export; **shipment** (+ style shipment,
  buy-order shipment); packing credit, packing / package / shipping info; port of
  loading; **cash incentive**; stamp amount; bank advice.
- Shipment → revenue recognition into the ledger (`GarmentsProfile.Sales`).
- **Done when:** an order is booked under LC, imported against, shipped, and the
  export proceeds + cash incentive hit the books.

### Phase G3 — Planning (TNA) + skill matrix *(1–2 months)*

- **TNA** (calendar, horizontal view, template, group update, responsible
  person), **process** (sequence, template, sequence default, key/sub/group
  sub-process), production line & daily line layout, **target production**,
  **capacity**, **efficiency rate**, working-day calendar, knitting / collar-cuff
  / yarn-dyeing programs.
- **Operator skill matrix** (moved out of HR) for line balancing.
- **Done when:** an order gets a TNA plan, a line assignment and a balanced
  process layout.

### Phase G4 — Production floor + quality *(3–5 months)*

- Port in value order: **cutting** (lay / roll / part / bundle / tag / grading /
  cut-bank / cut-fabric reject) → **sewing** (input / output process, SMV, key
  process) → **finishing** (iron / poly) → **knitting** (batch / roll / roll
  issue / machine / processor / grey register & issue / grey-delivery gatepass) →
  **dyeing** (job order / SP challan / dyes-chemical register / re-dyeing) →
  **embroidery / printing** (process + receive) → **subcontract** (fab
  sub-process delivery / receive challan).
- **`garments-quality`** — certificate, spec-sheet check, AQL / reject capture at
  each process; reject adjustment.
- WIP-by-process stock movements into `inventory`; batch / roll / lot tracking.
- Machine interruption / non-productive time → feeds `maintenance`.
- The HR integration seam (§04) — production-output export + worker-roster intake.
- SignalR production board (`ProductionHub`) + mobile-report API.
- **Done when:** an order is tracked cut-to-ship with quality gates.

### Phase G5 — Add-ons, report packs, mobile, retire SCERP *(ongoing)*

- **`maintenance`** (machine action / log / interruption, down-time category,
  maintenance report, vehicle + vehicle gate), **`crm`** (marketing inquiry /
  institute / sales contact / feedback), **`tasks`** (task board + notification
  board), **`gate`** (gate pass / vehicle / visitor gate entry).
- Rebuild every RMG report on EPPlus / iTextSharp — cost sheet, TNA status, SMV
  efficiency, line target/actual, shipment / export register, LC exposure, MIS
  commercial, plus the **ad-hoc "custom SQL query" report builder** and the
  **mobile-apps report** API.
- Migrate remaining SCERP customers; decommission SCERP.

---

## 08 · Effort & sequencing

| Phase | Scope | Rough size | Blocks on |
|---|---|---|---|
| G0 | template + masters + extend inventory/accounts + docs + notifications | ≈1 month | SCERP source (masters) |
| G1 | merchandising + all approval workflows + costing + order board | 2–3 months | G0; the two Angular grids |
| G2 | commercial / LC / trade | 2–3 months | G1; trade-finance domain knowledge |
| G3 | planning / TNA + skill matrix | 1–2 months | G0 |
| G4 | production floor + quality | 3–5 months | G3, G1; HR integration contract |
| G5 | maintenance + crm + tasks + gate + full report packs + mobile API + retire | 2–4 months | G1–G4 |

- **Credible garments MVP** (G0–G2 — a merchandiser + commercial + books tool):
  **~6–9 months** with the source in hand.
- **Full SCERP feature set minus HR:** realistically **18–30 engineer-months**.
  This is the honest number for "reuse every feature except HR" — SCERP is
  ~3,800 files and four of its domains do not exist in the platform.

The platform's own Phases 3–5 (POS, billing, breadth) run in parallel — garments
does not block them and vice-versa, except for shared frontend-shell work
(menu-from-`/api/me`, the Angular upgrade), which garments now depends on
heavily (14+ modules of lazy-loaded screens).

---

## 09 · Risks & guardrails

| Risk | Guardrail |
|---|---|
| **No source access** — cannot port from a file index | Get `SCERP.Model/.DAL/.BLL/.Web` + the DB / `SCERP.edmx`. Nothing real starts without it. |
| **Scale underestimate** — "reuse the modules" reads as config; it is a ~3,800-file / ~348-controller / 14-module port | This document. Phase it; ship G0–G2 as an MVP; Path B to de-risk. |
| **HR entanglement** in Production/Planning — skill matrix, org tree, working-day calendar, piece-rate output all live in the HR area | The §04 cut moves the non-payroll pieces out explicitly; a designed integration seam (output export + roster + availability intake) from day one. |
| **The approval workflows multiply** — sample, lab-dip, embellishment, trims each repeat dev → submit → approve with size/colour detail | Build one generic "approval submission" engine in `garments-merchandising`, configure the four flows on it, rather than porting four near-identical controllers sets. |
| **Cross-cutting subsystems** the platform lacks — document attachments, templated notifications, the ad-hoc SQL report builder | Build these as shared services in G0/G5, not per-module; the platform only has `Picture`, raw `MailService`/`SmsService`, and fixed reports today. |
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
6. **The piece-rate / loan decisions** from §04.

---

## Appendix · Coverage check — every SCERP area, nothing dropped except HR

| # | SCERP area (or sub-system) | In scope? | Lands in | Phase |
|---:|---|---|---|---|
| 1 | Merchandising — buyer / style / order | ✅ | `garments-merchandising` | G1 |
| 2 | Merchandising — sample / lab-dip / embellishment / trims approvals, spec sheet | ✅ | `garments-merchandising` (generic approval engine) | G1 |
| 3 | Costing — cost sheet, costing heads, consumption | ✅ | `garments-costing` | G1 |
| 4 | Tracking — order board, ready/sending status, process-status mail | ✅ | `garments-merchandising` + `notifications` | G1 |
| 5 | Commercial — Master LC / BB-LC / cash LC | ✅ | `garments-commercial` | G2 |
| 6 | Commercial — import / export / shipment / packing credit / cash incentive | ✅ | `garments-commercial` | G2 |
| 7 | Planning — TNA, process sequences, line layout, target, capacity, efficiency | ✅ | `garments-planning` | G3 |
| 8 | Planning — programs (knitting / collar-cuff / yarn-dyeing) | ✅ | `garments-planning` | G3 |
| 9 | Operator skill matrix *(was in HRM area)* | ✅ | `garments-planning` | G3 |
| 10 | Production — knitting | ✅ | `garments-production` | G4 |
| 11 | Production — dyeing (+ re-dyeing, dyes/chemical register) | ✅ | `garments-production` | G4 |
| 12 | Production — cutting (lay / roll / bundle / tag / grading / cut-bank) | ✅ | `garments-production` | G4 |
| 13 | Production — sewing (input / output, SMV, key process) | ✅ | `garments-production` | G4 |
| 14 | Production — finishing / iron / poly / embroidery / printing | ✅ | `garments-production` | G4 |
| 15 | Production — subcontract (fab sub-process delivery / receive) | ✅ | `garments-production` | G4 |
| 16 | Production — batch / roll / lot tracking, reject adjustment | ✅ | `garments-production` | G4 |
| 17 | Quality — certificate, spec-sheet check, AQL / reject | ✅ | `garments-quality` | G4 |
| 18 | Inventory — stores, requisition / issue / receive, GRN, returnable challan | ✅ | `inventory` (extended) | G0 |
| 19 | Inventory — booking (bulk / yarn / accessories), housekeeping store | ✅ | `inventory` (extended) | G0 |
| 20 | Purchase — store purchase, GRN against PO | ✅ | `purchase` | G0 |
| 21 | Accounting — CoA, control accounts, GL reparent | ✅ | `accounts` (extended) | G0 |
| 22 | Accounting — voucher entry (cash / bank / journal / contra / common) | ✅ | `accounts` | G0 |
| 23 | Accounting — cost centre (single + multi-layer), voucher segregation | ✅ | `accounts` (extended) | G0/G1 |
| 24 | Accounting — opening balance, financial period, bank rec, depreciation, AIT | ✅ | `accounts` (extended) | G0 |
| 25 | Accounting — multi-currency vouchers | ✅ | `accounts` (extended) | G0 |
| 26 | Common — geography tree (country → … → police station), port of loading | ✅ | `configuration` | G0 |
| 27 | Common — org tree (company → branch → unit → dept → section → line) *(part was in HRM)* | ✅ | `configuration` | G0 |
| 28 | Common — lookups (UOM, yarn count, fabric type, generic name, payment terms, party, supplier company) | ✅ | `configuration` | G0 |
| 29 | Common — working-day / holiday calendar *(was in HRM area)* | ✅ | `garments-planning` | G3 |
| 30 | Thin worker / operator master *(subset of Employee)* | ✅ | `configuration` | G0 |
| 31 | Email service + SMS service (2 stand-alone projects) + templates + process-status mail | ✅ | `notifications` | G0 |
| 32 | Document management — attachments on order / style / sample / lab-dip / trims | ✅ | cross-cutting attachment service | G0 |
| 33 | MIS — dashboards, MIS / commercial reports | ✅ | `report` (extended) | G5 |
| 34 | MIS — custom report + **ad-hoc SQL query builder** | ✅ | `report` (extended) | G5 |
| 35 | MIS — mobile-apps report API (a mobile client exists) | ✅ | `report` (extended) | G5 |
| 36 | Maintenance — machine action / log / interruption, down-time, maintenance report | ✅ | `maintenance` | G5 |
| 37 | Maintenance — vehicle master + vehicle gate | ✅ | `maintenance` + `gate` | G5 |
| 38 | CRM / Marketing — inquiry, institute, sales contact, feedback | ✅ | `crm` | G5 |
| 39 | Task Management — task board, assignee, follow-up, notification board | ✅ | `tasks` | G5 |
| 40 | Security / Gate — gate pass, visitor gate, grey-delivery gatepass | ✅ | `gate` | G5 |
| 41 | User Management — users / roles / permissions / menu / module-feature | ⚠️ **Replaced** | platform JWT + `RoleClaim` + `[RequiresModule]` + `/api/me` | done |
| 42 | User Management — user→merchandiser / user→line mapping | ✅ | `configuration` (as data) | G1/G3 |
| 43 | **HRM + Payroll** — attendance, leave, salary, bonus, penalty, OT, PF, gratuity, loan, roster, holiday admin, employee sub-records, HR hierarchy, job-card salary | ❌ **Excluded** | your external HR system; integration seam in `garments-production` | — |

Everything in SCERP is accounted for. The only ❌ is HRM/Payroll; the only ⚠️ is
User Management, which the platform already replaces with a better multi-tenant
equivalent.

---

*Garments vertical plan · v0.2 draft · 2026-09-07 · reconstructed from the SCERP
Visual Studio content index — source folders were not present on disk. Figures
are indicative pending access to the SCERP source and database.*
