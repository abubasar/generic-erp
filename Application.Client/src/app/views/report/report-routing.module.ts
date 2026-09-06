import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { hasPermission } from "app/shared/guards/has-permission.guard";
import { AccountsModuleReportComponent } from "./components/accounts-module-report/accounts-module-report.component";
import { ExcelUploadComponent } from "./components/excel-upload/excel-upload.component";
import { InventoryModuleReportComponent } from "./components/inventory-module-report/inventory-module-report.component";
import { PrimaryInventoryModuleReportComponent } from "./components/primary-inventory-module-report/primary-inventory-module-report.component";
import { PrimarySalesModuleReportComponent } from "./components/primary-sales-module-report/primary-sales-module-report.component";
import { ProductionModuleReportComponent } from "./components/production-module-report/production-module-report.component";
import { PurchaseModuleReportComponent } from "./components/purchase-module-report/purchase-module-report.component";
import { SalesModuleReportComponent } from "./components/sales-module-report/sales-module-report.component";

const routes: Routes = [
  {
    path: "accounts-module-report",
    canMatch: [
      () =>
        hasPermission([
          "Permissions.AccessReportModules.AccountsModuleReports",
        ]),
    ],
    component: AccountsModuleReportComponent,
    data: {
      module: "Report",
      pageTitle: "Accounts Module Report",
      breadcrumb: {
        title: "Accounts Module Report",
        url: "/report/accounts-module-report",
      },
    },
  },
  {
    path: "purchase-module-report",
    canMatch: [
      () =>
        hasPermission([
          "Permissions.AccessReportModules.PurchaseModuleReports",
        ]),
    ],
    component: PurchaseModuleReportComponent,
    data: {
      module: "Report",
      pageTitle: "Purchase Module Report",
      breadcrumb: {
        title: "Purchase Module Report",
        url: "/report/purchase-module-report",
      },
    },
  },
  {
    path: "production-module-report",
    canMatch: [
      () =>
        hasPermission([
          "Permissions.AccessReportModules.ProductionModuleReports",
        ]),
    ],
    component: ProductionModuleReportComponent,
    data: {
      module: "Report",
      pageTitle: "Production Module Report",
      breadcrumb: {
        title: "Production Module Report",
        url: "/report/production-module-report",
      },
    },
  },
  {
    path: "sales-module-report",
    canMatch: [
      () =>
        hasPermission(["Permissions.AccessReportModules.SalesModuleReports"]),
    ],
    component: SalesModuleReportComponent,
    data: {
      module: "Report",
      pageTitle: "Sales Module Report",
      breadcrumb: {
        title: "Sales Module Report",
        url: "/report/sales-module-report",
      },
    },
  },
  {
    path: "primary-sales-module-report",
    canMatch: [
      () =>
        hasPermission([
          "Permissions.AccessReportModules.PrimarySalesModuleReports",
        ]),
    ],
    component: PrimarySalesModuleReportComponent,
    data: {
      module: "Report",
      pageTitle: "Sales Module Report",
      breadcrumb: {
        title: "Sales Module Report",
        url: "/report/primary-sales-module-report",
      },
    },
  },
  {
    path: "inventory-module-report",
    canMatch: [
      () =>
        hasPermission([
          "Permissions.AccessReportModules.InventoryModuleReports",
        ]),
    ],
    component: InventoryModuleReportComponent,
    data: {
      module: "Report",
      pageTitle: "Inventory Module Report",
      breadcrumb: {
        title: "Inventory Module Report",
        url: "/report/inventory-module-report",
      },
    },
  },
  {
    path: "primary-inventory-module-report",
    canMatch: [
      () =>
        hasPermission([
          "Permissions.AccessReportModules.PrimaryInventoryModuleReports",
        ]),
    ],
    component: PrimaryInventoryModuleReportComponent,
    data: {
      module: "Report",
      pageTitle: "Inventory Module Report",
      breadcrumb: {
        title: "Inventory Module Report",
        url: "/report/primary-inventory-module-report",
      },
    },
  },
  {
    path: "excel-upload",
    canMatch: [() => hasPermission(["Permissions.Reports.Stock"])],
    component: ExcelUploadComponent,
    data: {
      module: "Report",
      pageTitle: "Data Import From Excel",
      breadcrumb: {
        title: "Data Import From Excel",
        url: "/report/excel-upload",
      },
    },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ReportRoutingModule {}
