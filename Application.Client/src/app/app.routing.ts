import { Routes } from "@angular/router";
import { AdminLayoutComponent } from "./shared/components/layouts/admin-layout/admin-layout.component";
import { AuthLayoutComponent } from "./shared/components/layouts/auth-layout/auth-layout.component";
import { AuthGuard } from "./shared/guards/auth.guard";

export const rootRouterConfig: Routes = [
  {
    path: "",
    redirectTo: "dashboard/analytics",
    pathMatch: "full",
  },
  {
    path: "",
    component: AuthLayoutComponent,
    children: [
      {
        path: "sessions",
        loadChildren: () =>
          import("./views/sessions/sessions.module").then(
            (m) => m.SessionsModule
          ),
        data: { title: "Session" },
      },
    ],
  },
  {
    path: "",
    component: AdminLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      {
        path: "dashboard",
        loadChildren: () =>
          import("./views/dashboard/dashboard.module").then(
            (m) => m.DashboardModule
          ),
        data: { title: "Dashboard", breadcrumb: "DASHBOARD" },
      },
      {
        path: "configuration",
        loadChildren: () =>
          import("./views/configuration/configuration.module").then(
            (m) => m.ConfigurationModule
          ),
        data: { title: "Configuration", breadcrumb: "Configuration" },
      },
      {
        path: "purchase",
        loadChildren: () =>
          import("./views/purchase/purchase.module").then(
            (m) => m.PurchaseModule
          ),
        data: { title: "Purchase", breadcrumb: "Purchase" },
      },
      {
        path: "production",
        loadChildren: () =>
          import("./views/production/production.module").then(
            (m) => m.ProductionModule
          ),
        data: { title: "Production", breadcrumb: "Production" },
      },
      {
        path: "sales",
        loadChildren: () =>
          import("./views/sales/sales.module").then((m) => m.SalesModule),
        data: { title: "Sales", breadcrumb: "Sales" },
      },
      {
        path: "accounts",
        loadChildren: () =>
          import("./views/accounts/accounts.module").then(
            (m) => m.AccountsModule
          ),
        data: { title: "Accounts", breadcrumb: "Accounts" },
      },
      {
        path: "report",
        loadChildren: () =>
          import("./views/report/report.module").then((m) => m.ReportModule),
        data: { title: "Report", breadcrumb: "Report" },
      },
      {
        path: "inventory",
        loadChildren: () =>
          import("./views/inventory/inventory.module").then(
            (m) => m.InventoryModule
          ),
        data: { title: "Inventory", breadcrumb: "Inventory" },
      },

      {
        path: "search",
        loadChildren: () =>
          import("./views/search-view/search-view.module").then(
            (m) => m.SearchViewModule
          ),
        data: { title: "Search", breadcrumb: "Search" },
      },
    ],
  },
  {
    path: "**",
    redirectTo: "sessions/404",
  },
];
