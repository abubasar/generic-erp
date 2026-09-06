import { Routes } from "@angular/router";

import { AnalyticsComponent } from "./analytics/analytics.component";
import { DashboardResolver } from "./resolvers/dashboard.resolver";

export const DashboardRoutes: Routes = [
  {
    path: "analytics",
    component: AnalyticsComponent,
    data: {
      module: "Analytics",
      pageTitle: "Analytics",
      breadcrumb: {
        title: "Analytics",
        url: "/dashboard/analytics",
      },
    },
    resolve: { dashboard: DashboardResolver },
  },
];
