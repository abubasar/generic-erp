# Adding the Garments (RMG) vertical to the ERP SaaS platform

**Enhancement plan — v1.1** *(v1.1: verified area-by-area against `SCERP.Web/Areas` + a full `grep` of the BLL — corrections: no "Quality" area (it's assembled); the cost sheet uses its own cost-group model, not the accounting cost centre; `Acc_CompanySector` / `CurrencyCommon` are config not ledger and are kept; the Production cutting job card ≠ the HRM `EmployeeJobCard`; machine/gate live in the Tracking area, returnable challan in Maintenance. v1.0: SCERP source read.)*

Bring the existing garments-manufacturing ERP ("SCERP") into the `GenericERP`
multi-tenant platform as a **Garments Industry** business template — reusing every
feature **except HRM/Payroll and Accounting**. Accounting is handled by the
platform's existing (Feed ERP) `accounts` module.

| | |
|---|---|
| **Requested** | Provision a garments-manufacturing tenant; reuse every SCERP feature except **HRM/Payroll** and **Accounting** (use the platform's Feed-ERP accounts instead). |
| **Source system** | `D:\MyDocuments\MyDocuments\Development\Development` — `SCERP.sln`: `SCERP.Model` / `SCERP.DAL` / `SCERP.BLL` / `SCERP.Common` / `SCERP.Web` (+ `SCERP.Mail`, `SCERP.Message.Service`). **.NET Framework 4.5.1**. |
| **Target** | `GenericERP` — **.NET 9** Clean Architecture API + **Angular** (current), EF Core code-first, pooled multi-tenancy, `BusinessTemplate` + `IIndustryProfile` + `[RequiresModule]`. |
| **Nature of the work** | **A rewrite to the modern stack, not a code migration.** No SCERP binary, project, DLL or `.cs` is referenced; each feature is re-implemented from the SCERP source on .NET 9 + EF Core + Angular. |
| **Prepared** | 2026-09-10 · from a full read of the SCERP source. |
| **Sibling doc** | `docs/saas-platform-plan.md` — the platform this plugs into. |
| **Interactive version** | https://claude.ai/code/artifact/afce3336-ea50-497a-8b57-1cce44aef440 |

---

## Contents

- [00 · Executive summary](#00--executive-summary)
- [01 · What SCERP is — measured](#01--what-scerp-is--measured)
- [02 · The accounting decision](#02--the-accounting-decision)
- [03 · Reuse map — SCERP area → platform module](#03--reuse-map--scerp-area--platform-module)
- [04 · The `garments` business template](#04--the-garments-business-template)
- [05 · "Except HRM and Accounts" — the precise cut](#05--except-hrm-and-accounts--the-precise-cut)
- [06 · Porting strategy — SCERP patterns → platform patterns](#06--porting-strategy--scerp-patterns--platform-patterns)
- [07 · Data & tenant strategy](#07--data--tenant-strategy)
- [08 · Phased roadmap](#08--phased-roadmap)
- [09 · Effort](#09--effort)
- [10 · Risks & guardrails](#10--risks--guardrails)
- [11 · What I need from you](#11--what-i-need-from-you)
- [Appendix · Coverage check](#appendix--coverage-check)

---

## 00 · Executive summary

**SCERP is a complete, mature RMG ERP** — order-to-ship for a knit/woven garments
factory: buyer → inquiry → sample/approval → style → buyer order → cost sheet →
consumption → booking → LC/import → TNA plan → knit/dye/cut/sew/finish → quality
→ shipment/export. Measured: **~437 controllers, ~384 business-logic managers,
~417 repositories, ~1,050 EF entities (353 `DbSet`s, a 54 k-line EDMX), ~2,900
Razor views, ~307 report definitions (~298 RDLC + ~9 Crystal).** ASP.NET MVC 5 / EF 6 database-first /
Crystal + RDLC / Handsontable / Bootstrap 3, on .NET Framework 4.5.1.

**Excluding HRM/Payroll and Accounting leaves ~291 controllers in scope** across
Merchandising (105), Production (43), Inventory (52), Commercial (24), Planning
(21), Common (15), and the small Tracking / Task / Maintenance / CRM / MIS /
Marketing areas — plus a handful of org-tree / skill-matrix / calendar entities
that physically live in the HRM model but are Planning/Config concerns.

**The accounting exclusion is clean.** SCERP's shop-floor, merchandising,
commercial and inventory managers **do not post to the general ledger** — no
auto-vouchers, no GL coupling. Accounting in SCERP is a separate manual
voucher-entry system (`Acc_VoucherMaster`/`Detail`, `Acc_GLAccounts`, cost
centres). The only cross-module accounting link is Payroll → salary vouchers,
and Payroll is excluded anyway. So SCERP's entire `Acc_*` model (33 entities, 26
controllers, its `AccountingManager`) is **dropped**, and the platform's
Feed-ERP `accounts` module is used unchanged, with **thin adapter services** for
the four money events garments produces (style payment received, export proceeds,
cash incentive, supplier/LC settlement).

**This is a multi-quarter re-platform, not a template tweak.** Realistic: a
**merchandiser-to-cost-sheet MVP in ~4–6 months** (G0–G2), **full in-scope
feature parity in ~16–26 engineer-months**. The one lucky break: SCERP entities
already carry a `string CompId` company discriminator on every row, which maps
directly onto the platform's `TenantId` during migration.

---

## 01 · What SCERP is — measured

| | Count | Notes |
|---|---:|---|
| Controllers | ~437 | ~291 in scope after excluding HRM (96) + Payroll (10) + Accounting (26) + User Management (9) |
| BLL "Manager" classes | ~384 | ctor-injected repositories, `PortalContext.CurrentUser.CompId` for company scope, `System.Transactions.TransactionScope` for multi-table writes |
| "Repository" classes | ~417 | `IRepository<T>` generic + per-entity repos with hand-written LINQ / SQL |
| EF entity types | ~1,050 | EF 6 **database-first** — a 54 k-line `SCERP.edmx`, 353 `DbSet`s in `SCERPDBContext` |
| Model `.cs` files | ~721 | generated partial entities + hand ViewModels + `*_Result` (stored-proc) types |
| Razor views | ~2,893 | server-rendered; **Handsontable** grids for matrices/cost sheets; jQuery |
| Report definitions | ~307 | ~298 **RDLC** (Microsoft.Reporting) + ~9 **Crystal** (`.rpt`) — one `Report` controller per area |
| Stand-alone services | 2 | `SCERP.Mail` (a trivial SMTP send helper) + `SCERP.Message.Service` (a **WCF** message host — email + SMS, own `MessageService.edmx`) |
| Runtime | — | **.NET Framework 4.5.1**, ASP.NET **MVC 5**, Autofac.Mvc5, AutoMapper, Bootstrap 3 |

### The pattern, from the source

```
Controller (Area)  →  IManager (BLL)  →  IRepository<T> / I…Repository (DAL)  →  SCERPDBContext (EF6 EDMX)
     Crystal/RDLC        TransactionScope        LINQ + raw SQL + SPs
     Handsontable        PortalContext.CurrentUser.CompId   (ambient company + user)
```

- **Every business entity has `string CompId`** — SCERP is already multi-company
  within one deployment. Filtering is manual (`x.CompId == _compId` in every
  manager query).
- **PKs are `long` / `int`; foreign keys are frequently `string`** (e.g.
  `OM_BuyerOrder.MerchandiserId`, `BuyerRefId` are strings).
- Controllers are large (400–800 lines) and mix data access, PDF/Excel/Crystal
  generation, and view-model shaping.
- No soft-delete filter, no audit trail, no row-level tenancy enforcement — a
  `CompId` mismatch is only caught by the manager remembering to filter.

### The 16 areas

The 16 MVC Areas, verified against `SCERP.Web/Areas`. **There is no "Quality" area** —
quality is assembled from `QualityCertificate` (Inventory), `SpecificationSheet`
(Merchandising) and the reject/grading controllers (Production). Some features are
filed under a surprising area (machine & gate are in **Tracking**, not Maintenance;
returnable challan is in **Maintenance**; TNA appears in Merchandising, Planning
*and* Commercial).

| Area | Ctrls | In scope? | What it does |
|---|---:|---|---|
| **Merchandising** | 105 | ✅ | buyer / agent / consignee / style / **buyer order (colour × size)** / order type / season / brand; **sample** + **lab-dip** + **embellishment** + **trims & accessories** (each: development → submission → approval, with size/colour detail + history + documents); **spec sheet**; **cost sheet** (`CostSheetTemplate` / `CostSheetMaster` / `CostSheetDetail` with template line-rates, `CostingHead`, `CostDefination` with a `CostGroup` string — **not** the accounting cost centre); **consumption** (fabric / yarn / thread / component) with cost; **style payment tracking** (`Acc_StylePayment` — a payment log, no GL lines); order-level TNA calendar; order-tracking board (order info, ready / sending status, process-status auto-mail); sticker / label printing |
| **Production** | 43 | ✅ | knitting (batch / roll / roll-issue / machine / processor / program / grey register & issue / grey-delivery gatepass); dyeing (job order / SP challan / dyes-chemical register / re-dyeing); cutting (lay / roll / part / bundle cutting, tag, grading, cut-bank, cut-fabric reject, cutting sequence) + the **cutting job card** (`SpCuttingJobCard` — bundle traceability, **distinct from the HRM `EmployeeJobCard`**); sewing (input / output process, **SMV** `StanderdMinValue`, key process); finishing (iron / poly); embroidery / printing (+ receive); subcontract (fab sub-process delivery / receive); **hourly production capture** (`Hour`); own & subcontractor factory masters (`Factory`, `DyeingFactory`); batch / roll / lot; reject adjustment; `ProductionHub` (SignalR) |
| **Inventory** | 52 | ✅ | stores (yarn / grey / finish fabric / accessories); item master (`InventoryItem`, `ItemStore`, `ItemMode`, `ItemType`); **material requisition → issue → receive** (general / advance / batch-wise / accessories / collar-cuff / fabric / yarn / grey); **GRN** (`GoodsReceivingNote`) / receive-against-PO / `MaterialReceived`; fabric & yarn return; store purchase & requisition; daily fabric receive; **booking** (bulk / yarn / accessories); **quality certificate** (`Inventory_QualityCertificate` + detail — QC at GRN); style shipment; approval status & authorised person; `LoanGiven` / `LoanReturn` *(decision needed — employee loan? inter-party?)* |
| **Commercial** | 24 | ✅ | Master **LC**, **BB-LC** (+ purchase, cash LC, cash BB-LC, cash-LC dyes/chemical), LC-order / LC-style; **import** + details + docs; **export**; **shipment**; **packing credit**, packing list; port of loading; **cash incentive**; bank advice (`CommBankAdvice` uses `CommAccHead`, a commercial-local bank head — not the GL); sales contact; commercial TNA; `CommImport` = a document tracker (LC value / invoice, no journal) |
| **Planning** | 21 | ✅ | **TNA** (calendar, horizontal, template, `TNAToTemplate`, group update, responsible person); **process** (sequence, template, sequence default, key / sub / group sub); **production line** & daily line layout; **target production**; programs (knitting / collar-cuff / yarn-dyeing); **working-day calendar** (`WorkingDay`) |
| **Common** | 15 | ✅ | measurement unit, payment terms, colour, size, party, `CurrencyCommon`; geography (`City`, `State`); **`Document`** + **email templates** (`EmailTemplate` / `EmailTemplateUser` / `EmailUser`) + `MailSend`; **`GatePass`**; **custom report** + **`CustomSqlQuary`** (ad-hoc SQL report builder); `UserReport` |
| **Tracking** | 8 | ✅ | `ConfirmationMedia`; **`HouseKeeping`** (consumables → `inventory`); **machine** action & log (→ `maintenance`); **`Vehicle`**, **`VehicleGateEntry`**, **`VisitorGateEntry`** (→ `gate`); `VisitorReport` |
| **Task Management** | 7 | ✅ | task + status + type, assignee, subject; `ReportImage`; a stray `Module` controller |
| **Maintenance** | 4 | ✅ | `MaintenanceReport`; **`ReturnableChallan`** / receive / receive-master (a returnable-goods challan — belongs in `inventory`) |
| **CRM** | 4 | ✅ | `Feedback`, `ProjectDocumentInfo` (→ attachment service), a `Report` controller |
| **MIS** | 5 | ✅ | `MisDashboard`, `MisReport`, `MisCommercialReport`, **`MobileAppsReport`** API (a mobile client consumes it), `UserActivity` (audit → platform `EventLog`) |
| **Marketing** | 3 | ✅ | `MarketingInquiry`, `MarketingInstitute`, `MarketingReports` |
| **Accounting** | 26 | ❌ | CoA, control accounts, voucher entry (cash/bank/journal/contra/common), voucher list & cost-centre segregation, cost centre (single + multi-layer), opening/closing balance, financial period, bank reconciliation, depreciation chart, multi-currency, advanced income tax → **replaced by the platform `accounts` module** (§02). *Exception:* `Acc_CompanySector` / `Acc_ActiveCompanySector` (company/sector identity for report letterheads) and `CurrencyCommon` are **config, not ledger** — they map to the platform's `Company` / `Currency` and are kept. |
| **User Management** | 9 | ❌ | users / roles / permissions / menu / module-feature / department- & employee-level permission (+ the root `AccountController` = ASP.NET login) → **replaced by platform JWT + `RoleClaim` + `[RequiresModule]` + `/api/me`** |
| **HRM** | 96 | ❌ | employee master + ~30 sub-records, attendance, leave, `EmployeeJobCard` (salary side), work shift/group/roster, holiday admin, skill matrix\*, org tree\*, efficiency\*, geography\*, HR lookups\* → **excluded** (\* = the starred pieces move out — see §05) |
| **Payroll** | 10 | ❌ | salary setup / mapping / process / search / increment / advance, bonus, penalty, OT, PF, gratuity, pay slip → **excluded** |

---

## 02 · The accounting decision

**Your instruction — "for accounts, follow the existing Feed ERP system" — is
both correct and cheap to execute, because SCERP's accounting is not wired into
anything else.**

### What the source shows (verified — a `grep` of every BLL manager and every Web controller)

- **No non-accounting BLL manager references `Acc_VoucherMaster`, `Acc_VoucherDetail`,
  `Acc_GLAccounts`, `Acc_ControlAccounts`, `Acc_CostCentre`, `Acc_OpeningClosing`
  or `Acc_FinancialPeriod`** — every hit is inside `AccountingManager/` or the
  DI composition root. Merchandising, Production, Inventory, Commercial and
  Planning managers do their operational writes and stop.
- The **only** GL-posting coupling from another area is `Payroll →
  EmployeeSalaryProcessConfirmManager` (posts salary vouchers) +
  `SalaryMappingController` (maps salary heads to GL) — both excluded.
- The one non-accounting Web controller that touches the `AccountingModel`
  namespace is `Commercial/LcController` — and only for `Acc_CompanySector`
  (the company letterhead for the LC report). Not a ledger call. That master
  moves to `configuration` (see below).
- `Acc_StylePayment` (managed by `AccountingManager/StylePaymentManager`, driven
  from `Merchandising/StylePaymentController`) is a **payment log** — `PayDate`,
  `OrderNo`, `OrderStyleRefId`, `CostGroup`, no debit/credit lines.
- `CommImport` / LC entities are **document trackers** — `LcValue`, `DocsValue`,
  `InvoiceNo`, `BillOfEntry` — with no journal side.
- The cost sheet uses its **own** `CostGroup` string + `CostingHead`, **not**
  `Acc_CostCentre` — so dropping the accounting cost centre costs nothing here.

### What this means

| Drop from SCERP | Use from the platform instead |
|---|---|
| `Acc_*` ledger model — `VoucherMaster` / `VoucherDetail` / `GLAccounts` / `GLAccounts_Hidden` / `ControlAccounts` / `CostCentre` / `CostCentreMultiLayer` / `Currency` / `FinancialPeriod` / `OpeningClosing` / `BankReconcilation*` / `DepreciationChart` / `VoucherToCostcentre` / `VoucherLimit` / `PermitedChartOfAccount` / `SalaryMapping` / `StylePayment` (~33 entities) | `Application.Core/Entities` — `Account` (the shared CoA skeleton), `AccountType`, `CostCenter`, `Currency`, `JournalEntry`, `VoucherEntry`, `PaymentVoucher`, `ReceiveVoucher`, `ReceivePayment` / `ReceivePaymentAgainstSale`, `FundTransfer`, `Transaction`, `BankAccount` |
| Accounting Area (26 controllers) + `AccountingManager` | `Application.Api/Controllers` accounts + report controllers, `AccountService` / `AccountReportPdfService` — already `[RequiresModule("accounts")]`, already the "keystone asset" per the SaaS plan |
| SCERP cost centres (multi-layer) | `CostCenter` — the platform's cost centre. The garments cost *sheet* does **not** use it (own `CostGroup` model); verify voucher-level cost-centre depth covers the garments dimensions, extend if not (small). |
| SCERP multi-currency vouchers | platform `Currency` + voucher lines; garments needs FX on the buyer order (`OM_BuyerOrder.CurrencyId` / `Exchange`) — that stays operational in `garments-merchandising`, settled through platform vouchers |

**Kept, not dropped** (they sit in the `AccountingModel` namespace but are not
ledger): `Acc_CompanySector` / `Acc_ActiveCompanySector` (company & sector
identity for report letterheads, used by Commercial and others) → the platform's
`Company` / `Tenant` profile; `CurrencyCommon` → platform `Currency`.

### The adapter seams (thin services in `garments-*`, not a module)

Garments produces four kinds of money event. Each becomes a small service that
calls the existing accounts services — no new ledger:

| Event | Source module | Posts through |
|---|---|---|
| Buyer payment received against an order/style | `garments-merchandising` (was `StylePayment`) | `ReceivePayment` / `ReceivePaymentAgainstSale` (AR) |
| Export proceeds realised on a shipment | `garments-commercial` | `ReceiveVoucher` / `ReceivePayment` (AR + bank) |
| Cash incentive received | `garments-commercial` | `ReceiveVoucher` (other income) |
| LC / BB-LC settlement, import payment, subcontract bill | `garments-commercial` / `garments-production` | `PaymentVoucher` / `SupplierPayment` (AP) |

Everything else stays operational (LC docs, import tracking, cost sheets) and
never touches the ledger — exactly as in SCERP today.

### Chart-of-accounts

The platform ships a shared CoA skeleton (`ITenantSharable`, ~55 rows). The
garments seed pack adds a **CoA overlay** — WIP-by-process, LC margin, cash-
incentive receivable, packing-credit liability, subcontract-charges — as
tenant-scoped children under the shared roots.

---

## 03 · Reuse map — SCERP area → platform module

Platform catalog today: `configuration · inventory · purchase · sales ·
production · accounts · report`. Garments **reuses 4, extends 2, adds a
`garments-*` family + cross-cutting add-ons**, and **replaces 3** (accounts,
user-management, HRM):

| Module | Key | Disposition | From SCERP |
|---|---|---|---|
| Configuration & masters | `configuration` | **Reuse + extend** | Common area + the org tree / geography / lookups that sit in the HRM model |
| Inventory & stock | `inventory` | **Reuse + extend** | Inventory area — garment stores, requisition/issue/receive variants, returnable challan, booking, housekeeping |
| Purchase | `purchase` | **Reuse** | store purchase / GRN against PO |
| **Accounting** | `accounts` | **Reuse unchanged** — the platform's Feed-ERP module | *(SCERP `Acc_*` dropped; adapter seams only — §02)* |
| Reports & analytics | `report` | **Reuse + extend** | MIS — dashboards, the ~307 report definitions (rebuilt), the ad-hoc SQL builder, the mobile-report API |
| Merchandising | `garments-merchandising` | **New** | Merchandising + Tracking — buyer order (colour × size), style, all approval workflows, spec sheet, order board, style-payment (→ AR adapter) |
| Costing | `garments-costing` | **New** | `CostSheetTemplate` / `Master` / `Detail` (template line-rates), `CostingHead`, `CostDefination` (+ `CostGroup`), per-style consumption + cost, margin — **its own cost-group model, no accounting cost centre** |
| Commercial / trade | `garments-commercial` | **New** | Commercial — Master LC, BB-LC, import, export, shipment, packing credit, cash incentive, port of loading (+ AP/AR adapters) |
| Planning | `garments-planning` | **New** | Planning + skill matrix / efficiency / working-day calendar (from HRM model) |
| Production floor | `garments-production` | **New** | Production — knit / dye / cut / sew (SMV) / finish / embroidery / print / subcontract / batch-roll-lot / reject |
| Quality | `garments-quality` | **New** | assembled — `Inventory_QualityCertificate` (QC at GRN), `SpecificationSheet` (merchandising), reject / grading (production). **Not a lifted area** — SCERP has no Quality area. |
| Notifications | `notifications` | **New** | `SCERP.Mail` + `SCERP.Message.Service` — templated email/SMS + event triggers + process-status auto-mail (platform has raw `MailService`/`SmsService` only) |
| Document attachments | *(cross-cutting service)* | **New** | SCERP `*Document` folders — files on order / style / sample / lab-dip / trims |
| Maintenance · CRM · Tasks · Gate | `maintenance` · `crm` · `tasks` · `gate` | **New add-ons** | Maintenance / CRM / Marketing / Task Management / gate-pass |
| — User Management | *(none)* | **Replaced** | platform JWT + `RoleClaim` + `[RequiresModule]` + `/api/me` |
| — HRM / Payroll | *(none)* | **Excluded** | your external HR system + a production-output / roster integration seam (§05) |

Dependencies: `garments-*` all `DependsOn = "inventory"`; `garments-costing` on
`garments-merchandising`; `garments-planning` on `garments-merchandising`;
`garments-production` on `garments-planning,garments-merchandising`;
`garments-quality` on `garments-production`; `garments-commercial` on
`garments-merchandising,purchase,accounts`.

---

## 04 · The `garments` business template

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

**`GarmentsProfile : IIndustryProfile`** (mirrors `PharmacyProfile` / `FeedProfile`):

| Member | Garments behaviour |
|---|---|
| `Uom` | Fabric in Kg / yds / metres, yarn in lbs / cones, garments in dozens / pcs — richer `IUomPolicy` than feed's single bags↔Kg; `OM_BuyerOrder` already carries `GUOMId` / `GUOMConv` / `BasUnit` |
| `Sales` | No walk-in sale; "sale" = shipment against a buyer order; revenue on shipment / export realisation; buyer order carries currency + exchange rate |
| `Reports` | RMG report pack — cost sheet, TNA status, sewing efficiency (SMV), line target vs. actual, shipment / export register, LC exposure |
| `Dashboard` | Order book, WIP by process, on-time-delivery %, line / machine utilisation, LC exposure |

**Seed pack** (added to `ProvisioningService`, on top of financial-year / roles /
units / store / owner-user): garment UOMs + conversions, default costing-head
set, default TNA template, standard process sequence (knit → dye → cut → sew →
finish → ship), one production line, the garments CoA overlay (§02).

---

## 05 · "Except HRM and Accounts" — the precise cut

### Accounting — see §02. All `Acc_*` dropped; platform `accounts` used; four adapter seams.

### HRM — what moves out, what's dropped

The HRM model is 137 entities. Several are **not** HR and are needed by
Planning / Production / Configuration — they move out rather than being lost:

| Moves out of HRM — kept | Goes to | Why |
|---|---|---|
| Org tree — `Company`, `Branch`, `BranchUnit`, `BranchUnitDepartment`, `Department`, `DepartmentLine`, `DepartmentSection`, `Section`, `Line`, `HeadOfDepartment`, `CompanyOrganogram` | `configuration` | production, inventory stores and cost centres all reference it |
| Geography — `Country`, `District`, `PoliceStation` | `configuration` | addresses on buyer / supplier / party |
| Skill matrix — `HrmSkillMatrix`, `HrmSkillMatrixDetail`, `HrmSkillMatrixGrade`, `HrmSkillMatrixProcess`, `SkillLevel`, `SkillOperation`, `SkillSetCategory`, `SkillSetDifficulty`, `EmployeeSkill`, `EfficiencyRate` | `garments-planning` | line balancing and process assignment |
| Working-day / holiday calendar — `GeneralDaySetup`, `ExceptionDay`, `OutStationDuty` (the calendar, not leave admin) | `garments-planning` | TNA date arithmetic |
| Housekeeping — `HouseKeepingItem`, `HouseKeepingRegister` | `inventory` | consumables store |
| HR lookups — `Gender`, `Religion`, `BloodGroup`, `MaritalState`, `EducationLevel` | `configuration` (cheap) | referenced by the thin worker master |
| A thin **worker / operator master** — id, name, card no, line, grade, skill — a subset of `Employee` | `configuration` | Production records output per operator; SMV needs a headcount |

**Dropped** (kept in your external HR system, ~110 entities / 96 + 10 controllers):
employee master's ~30 sub-records; attendance (daily / in-out / manual / machine
import); `EmployeeJobCard` + `EmployeeJobCardProcess` (the salary side); leave
(application / approval / recommendation / types / settings / maternity / short /
outstation duty); salary (setup / mapping / process / search / increment /
advance); compensation (bonus / attendance bonus / penalty / OT / PF / gratuity);
roster (work shift / group / roster, holiday admin); appraisal; quit /
separation; employee card print.

**Not HRM despite the name — in scope:**

- **`Production/JobCardController` + `SpCuttingJobCard`** — the *cutting job card*
  (bundle-to-operation traceability on the shop floor, driven by
  `CuttingBatchManager` / `LayCuttingManager` / `RollCuttingManager`). This is
  `garments-production`, **not** the HRM `EmployeeJobCard`.
- **`Inventory/LoanGiven` + `LoanReturn`** — a returnable-goods / advance-material
  loan register, sitting in the Inventory area. **Decide:** if it's employee loans
  → drop with HR; if it's inter-party material loans → keep in `inventory`.

**One decision for you:** sewing *output* is kept (production data —
`PROD_SewingOutPutProcess`); the *piece-rate salary calc* off it is dropped and
belongs to your payroll. Confirm that split.

### Integration seam — built into `garments-production` from day one

- `GET /api/garments/production-output` — a webhook/export your payroll polls:
  line × operator × style × process × qty × date × SMV.
- `PUT /api/garments/worker-roster` — your HR pushes the active worker list.
- `PUT /api/garments/line-availability` — daily present-count per line, so
  Planning has capacity without owning HR (or those numbers are entered by hand).

---

## 06 · Porting strategy — SCERP patterns → platform patterns

| SCERP (.NET Framework MVC5) | → | `GenericERP` (.NET 9) |
|---|---|---|
| **EF 6 database-first** — `SCERP.edmx`, 1,050 entities, `long`/`int` PKs, `string` FKs, `string CompId` on every row | | **EF Core code-first** — entities in `Application.Core/Entities/Garments/<Area>/`, `Guid` PKs, real FK navs, `Guid TenantId` via `ITenantScoped` (the global query filter replaces the manual `x.CompId == _compId`) |
| **~384 `Manager` (BLL)** — ctor-injected repos, `PortalContext.CurrentUser.CompId`, `TransactionScope` | | `BaseService<TEntity, …>` + `BaseRepository` + `IUnitOfWork` (audit + soft-delete + cross-tenant guard); one Autofac module per garments sub-domain. Most managers are CRUD + paging → collapse into `BaseService.SearchAsync`; port only the real logic |
| **~417 `Repository`** — generic + hand LINQ/SQL + stored procs | | `BaseRepository<T>` + a few `ExecuteStoredProcedureAsync` for the report SPs that are worth keeping |
| **~437 MVC5 controllers**, 16 Areas, server Razor | | `[ApiController]` REST under `Application.Api/Controllers/Garments/`, each `[RequiresModule("garments-…")]`; Angular screens in the tenant shell |
| **`PortalContext.CurrentUser`** (ambient company + user, session-backed) | | `TenantScope.CurrentTenantId` (JWT) + `IWorkContext` / `ITenantContext` (DI) |
| **ASP.NET Identity + a UserManagement Area** (roles, menu, module-feature, dept/emp-level permissions) | | platform JWT + `RoleClaim` + `[Authorize("Permission")]` + `[RequiresModule]` + `/api/me`-driven menu — **do not port** |
| **~307 report definitions** — ~298 RDLC + ~9 Crystal `.rpt` | | EPPlus (Excel) + iTextSharp (PDF) — the platform's stack; every report costed individually, rebuild only what customers use; the "custom SQL query" screen buys time for the long tail |
| **Handsontable** grids — colour × size matrix, cost sheet, line layout | | Angular data-grid components; the **colour × size order grid** and the **cost sheet** are the two hardest UI pieces |
| **`SCERP.Mail` + `SCERP.Message.Service`** (a WCF message host, own `MessageService.edmx`, email + SMS) | | `notifications` module — templates + an outbox + a hosted `BackgroundService`; platform `MailService` / `SmsService` are the transports |
| No tenancy / soft-delete / audit anywhere | | every ported entity through the Phase-0 pipeline — `ITenantScoped` (or `ITenantSharable` for system lookups), global query filter, `UnitOfWork` audit. Non-negotiable. |

**Naming:** SCERP prefixes entities `OM_` (merchandising), `PROD_` (production),
`PLAN_` (planning), `Comm*` (commercial), `Acc_` (dropped). Drop the prefixes;
namespace by folder (`Application.Core/Entities/Garments/Merchandising/BuyerOrder.cs`).

**Shared lookups:** SCERP's currency / UOM / country / port merge into the
platform `configuration` masters as `ITenantSharable` system data (same mechanism
as the shared CoA skeleton), so every garments tenant starts with them.

---

## 07 · Data & tenant strategy

**Two paths — pick one before design starts.**

### Path A — Full port

Re-platform the in-scope modules; migrate existing SCERP customers tenant by
tenant. **The `string CompId` on every SCERP row becomes the `TenantId`** — a
one-to-one map, which makes the data migration far cleaner than it was for the
POS. The only path that ends with one codebase.

- **Pro:** one platform, self-service onboarding, per-module pricing.
- **Con:** ~16–26 engineer-months to in-scope parity.
- **Useful MVP:** `configuration` + `inventory` + `accounts` +
  `garments-merchandising` + `garments-costing` — a merchandiser's
  order-to-cost-sheet system with stock and books, before the shop floor lands.

### Path B — Connected silo first

Keep SCERP running for existing customers; provision a `garments` tenant shell on
the platform now (masters + accounts + inventory + merchandising MVP); sync
orders / shipments / production output between them; port module by module and
cut SCERP over piece by piece.

- **Pro:** a garments tenant in weeks; de-risks the port; learn the domain.
- **Con:** an integration layer + two running systems for a while.

> **Recommendation:** Path B to get live and learn, Path A running behind it. Do
> not big-bang. The `garments` template + `GarmentsProfile` + seed pack are built
> first regardless — small, and they unblock provisioning.

---

## 08 · Phased roadmap

Slots after the platform's Phase 2 (self-service onboarding).

### G0 — Template, masters, extend reused modules, cross-cutting services *(≈1 month)*

- `garments` `BusinessTemplate` + `GarmentsProfile` + module catalog + price-book
  entries + garments seed pack in `ProvisioningService`.
- `configuration` extend: org tree, geography, garment lookups (yarn count,
  fabric type, generic name, payment terms, party, supplier company, currency,
  UOM + conversions), thin worker master.
- `inventory` extend: garment store types, requisition → issue → receive variants,
  returnable challan, booking, housekeeping store.
- `accounts`: verify cost-centre depth; add the garments CoA overlay to the seed
  pack. **No SCERP accounting code.**
- Build the tenant-scoped **document-attachment** service and the
  **`notifications`** module (templates + outbox + background sender).
- **Done when:** a garments tenant is provisioned, logs in, runs Inventory /
  Purchase / **the Feed-ERP accounts module**, with garment masters and attachments.

### G1 — Merchandising + costing MVP *(2–3 months)*

- Buyer / agent / consignee / party; style (colour × size); **buyer order
  (colour × size grid)** with currency + exchange.
- **All approval workflows** on one generic approval engine — sample, lab-dip,
  embellishment, trims & accessories; spec sheet.
- **Costing** — `CostingHead`, `CostDefination` (+ `CostGroup`), cost-sheet
  template / master / detail (template line-rates), per-style consumption
  (fabric / yarn / thread / component) with cost, margin. *(No accounting cost
  centre — the cost sheet has its own cost-group model.)*
- **Style payment** → the AR adapter (posts a platform `ReceivePayment`).
- **Order tracking board** + process-status auto-mail.
- Angular: the buyer-order grid and the cost-sheet grid.
- **Done when:** inquiry → sample approval → order → cost-sheet → buyer payment,
  end to end.

### G2 — Commercial / trade *(2–3 months)*

- Master LC, BB-LC (+ cash LC, cash BB-LC, cash-LC dyes/chemical), LC-order /
  LC-style; import + details + docs; export + details; **shipment**; packing
  credit + packing list; port of loading; **cash incentive**; bank advice.
- Adapters: LC/import settlement → `PaymentVoucher`; export proceeds + cash
  incentive → `ReceiveVoucher`.
- **Done when:** an order is booked under LC, imported against, shipped, and the
  proceeds + incentive land in the Feed-ERP ledger via the adapters.

### G3 — Planning (TNA) + skill matrix *(1–2 months)*

- TNA (calendar, horizontal, template, responsible person); process (sequence /
  template / key / sub / group sub); production line & daily layout; target
  production; capacity; programs; working-day calendar.
- Operator skill matrix + efficiency rate.
- **Done when:** an order gets a TNA plan, a line assignment and a balanced layout.

### G4 — Production floor + quality *(3–5 months)*

- Value order: cutting (lay / roll / part / bundle cutting, tag, grading,
  cut-bank, cut-fabric reject, cutting sequence) **+ the cutting job card**
  (`SpCuttingJobCard`) → sewing (input / output, SMV, key process) → finishing
  (iron / poly) → knitting → dyeing → embroidery / print → subcontract.
- Hourly production capture (`Hour`); own & subcontractor factory masters.
- `garments-quality` — `Inventory_QualityCertificate` (QC at GRN), spec-sheet
  check, reject / grading at each process.
- WIP-by-process movements into `inventory`; batch / roll / lot tracking.
- Machine action / log / interruption / non-productive time → `maintenance`.
- The HR integration seam (§05); SignalR production board + mobile-report API.
- **Done when:** an order is tracked cut-to-ship with quality gates.

### G5 — Add-ons, report packs, retire SCERP *(2–4 months, ongoing)*

- `maintenance`, `crm`, `tasks`, `gate`.
- Rebuild the RMG report pack on EPPlus / iTextSharp — cost sheet, TNA status,
  SMV efficiency, line target/actual, shipment / export register, LC exposure —
  plus the ad-hoc SQL builder and the mobile-apps report API.
- Migrate remaining SCERP customers (`CompId` → `TenantId`); decommission SCERP.

---

## 09 · Effort

| Phase | Scope | Rough size |
|---|---|---|
| G0 | template + masters + extend inventory/accounts + docs + notifications | ≈1 month |
| G1 | merchandising + approvals + costing + order board + AR adapter | 2–3 months |
| G2 | commercial / LC / trade + AP/AR adapters | 2–3 months |
| G3 | planning / TNA + skill matrix | 1–2 months |
| G4 | production floor + quality | 3–5 months |
| G5 | add-ons + ~307 reports + retire | 2–4 months |

- **Merchandiser-to-cost-sheet MVP** (G0–G2): **~4–6 months** with the source in
  hand *(faster than the full-SCERP estimate because accounting is out and the
  `CompId`→`TenantId` map is clean)*.
- **Full in-scope feature parity minus HRM & Accounts:** **~16–26
  engineer-months.** The report rebuild (~307 defs) and the two hard Angular grids
  are the schedule risks.

The platform's own Phases 3–5 run in parallel; garments depends heavily on the
shared tenant-shell work (menu-from-`/api/me`, the Angular upgrade) since it adds
14+ modules of lazy-loaded screens.

---

## 10 · Risks & guardrails

| Risk | Guardrail |
|---|---|
| **Scale** — ~291 in-scope controllers, ~1,050 entities, ~2,900 views, ~307 reports; four net-new domains | Phase it; ship G0–G2 as an MVP; Path B to de-risk. Not a big-bang. |
| **EDMX → EF Core** — 1,050 EF6 entities, `string` FKs, `long` PKs, no navs | Do not machine-translate the EDMX. Re-model per garments sub-domain with `Guid` PKs and real navs; keep a `LegacyId` column per entity during migration to remap the `string`/`long` references. |
| **~307 report definitions (~298 RDLC + ~9 Crystal)** — each a manual rebuild | Cost each; rebuild only reports customers use; the "custom SQL query" screen covers the long tail; treat the report pack as its own G5 workstream. |
| **Costing & consumption engine** — the real IP; wrong here = wrong quotes | Port with the original SCERP developer / a merchandising domain expert; golden-file test the new engine against SCERP output on real orders. |
| **The approval workflows repeat** (sample / lab-dip / embellishment / trims) | One generic "approval submission" engine, four configured flows — not four controller sets. |
| **Accounting adapters drift** from the operational data | The adapter is the *only* write path from garments to the ledger; each posts idempotently keyed on the source document id; reconcile the order-vs-ledger position nightly. |
| **Trade finance (LC / BB-LC)** is regulated and bank-specific | `garments-commercial` is its own hardening project; don't MVP it loosely. |
| **Tenancy debt** — SCERP enforces `CompId` only by manager convention | Every ported entity gets `ITenantScoped` + the global query filter + the `UnitOfWork` guard. The `CompId` field is dropped after migration maps it to `TenantId`. |
| **Two running systems** (Path B) | One system of record per fact (masters + books = platform; shop floor = SCERP until ported); idempotent one-way sync; nightly reconcile. |

---

## 11 · What I need from you

1. **Path A vs Path B** (§07) — full port, or connected-silo-first.
2. **The HR integration contract** — what your external payroll needs from
   production output; what worker / roster / availability data it returns.
3. **The piece-rate decision** (§05) — confirm sewing output stays, salary calc goes.
4. **Priority customer & modules** — which garments factory goes first, and which
   of merchandising / commercial / planning / production they most need. Sets the
   G1–G4 order.
5. **"Parity or MVP"** for the first release — a merchandiser's
   order-to-cost-sheet tool (G0–G2), or nothing until the shop floor is in (G0–G4).
6. **The SCERP database** — a backup, so the `CompId` → `TenantId` migration and
   the golden-file costing tests can be built.

---

## Appendix · Coverage check — every SCERP area, nothing dropped except HRM & Accounts

| SCERP area / sub-system | In scope | Lands in | Phase |
|---|---|---|---|
| Merchandising — buyer / style / order / all approval workflows / spec sheet / order board | ✅ | `garments-merchandising` | G1 |
| Costing — cost sheet / costing heads / consumption | ✅ | `garments-costing` | G1 |
| Style payment (buyer receipts) | ✅ | `garments-merchandising` → **`accounts` AR adapter** | G1 |
| Commercial — Master LC / BB-LC / cash LC / import / export / shipment / packing credit / cash incentive | ✅ | `garments-commercial` (+ `accounts` AP/AR adapters) | G2 |
| Planning — TNA / process / line layout / target / capacity / programs | ✅ | `garments-planning` | G3 |
| Skill matrix / efficiency / working-day calendar *(from HRM model)* | ✅ | `garments-planning` | G3 |
| Production — knit / dye / cut / sew (SMV) / finish / embroidery / print / subcontract / batch-roll-lot / reject | ✅ | `garments-production` | G4 |
| **Cutting job card** (`SpCuttingJobCard` — ≠ HRM `EmployeeJobCard`) + hourly capture + factory masters | ✅ | `garments-production` | G4 |
| Quality — `Inventory_QualityCertificate` (QC at GRN) + `SpecificationSheet` (merch) + reject/grading (prod). *SCERP has no Quality area — assembled.* | ✅ | `garments-quality` | G4 |
| Inventory — stores / requisition → issue → receive / GRN / booking; `HouseKeeping` (from Tracking); returnable challan (from Maintenance area) | ✅ | `inventory` (extended) | G0 |
| Purchase — store purchase / GRN against PO | ✅ | `purchase` | G0 |
| Common — geography (`City`/`State`) / lookups (UOM, colour, size, payment terms, party) / `CurrencyCommon` | ✅ | `configuration` | G0 |
| Org tree — company / branch / unit / dept / section / line / organogram *(sits in the HRM model)* | ✅ | `configuration` | G0 |
| Thin worker / operator master *(subset of `Employee`)* + HR lookups (gender / religion / …) | ✅ | `configuration` | G0 |
| `Acc_CompanySector` / `Acc_ActiveCompanySector` *(letterhead identity, in the AccountingModel)* | ✅ | `configuration` (→ `Company` / `Tenant`) | G0 |
| `SCERP.Mail` + `SCERP.Message.Service` — templated email/SMS + process-status mail + `EmailTemplate*` | ✅ | `notifications` | G0 |
| Document management — `Document` / `CommFileUpload` / `ProjectDocumentInfo` / `Sticker` / order-sample-labdip-trims docs | ✅ | attachment service | G0 |
| MIS — `MisDashboard` / `MisReport` / `MisCommercialReport` / ad-hoc `CustomSqlQuary` / `MobileAppsReport` API | ✅ | `report` (extended) | G5 |
| Maintenance — `MaintenanceReport` + machine action / log / interruption / non-productive-time *(machine controllers are in the Tracking area)* | ✅ | `maintenance` | G5 |
| CRM — `Feedback` / `ProjectDocumentInfo`; Marketing — `MarketingInquiry` / `MarketingInstitute` / `MarketingReports`; `SalesContact` *(in Commercial)* | ✅ | `crm` | G5 |
| Task Management — `Task` / status / type / assignee / subject; `NotificationBoard` *(in Merchandising)* | ✅ | `tasks` | G5 |
| Gate — `GatePass` *(Common)* / `VehicleGateEntry` / `VisitorGateEntry` *(Tracking)* | ✅ | `gate` | G5 |
| **Accounting** — CoA / vouchers / cost centre / bank rec / depreciation / multi-currency / financial period / `StylePayment` / `SalaryMapping` | ❌ **Replaced** | the platform's **Feed-ERP `accounts` module**, unchanged; garments posts via 4 adapter seams (§02) | done |
| User Management — users / roles / permissions / menu / module-feature (+ root `AccountController` = login) | ❌ **Replaced** | platform JWT + `RoleClaim` + `[RequiresModule]` + `/api/me` | done |
| **HRM + Payroll** — attendance / leave / salary / bonus / penalty / OT / PF / gratuity / roster / holiday admin / employee sub-records / appraisal / `EmployeeJobCard` | ❌ **Excluded** | your external HR + integration seam in `garments-production` | — |

Only two exclusions (HRM/Payroll, Accounting) and one replacement (User
Management) — everything else is in scope.

---

*Garments vertical plan · v1.1 · 2026-09-10 · from a full read of the SCERP source
(`D:\MyDocuments\MyDocuments\Development\Development`). SCERP is .NET Framework
4.5.1 / MVC5 / EF6 database-first / Crystal + RDLC; the target is .NET 9 + Angular
+ EF Core — this is a rewrite, not a migration. Effort figures are indicative
pending the SCERP database backup and a priority-customer decision.*
