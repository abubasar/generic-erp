import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FlexLayoutModule } from "@angular/flex-layout";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import { SharedComponentsModule } from "app/shared/components/shared-components.module";
import { SharedPipesModule } from "app/shared/pipes/shared-pipes.module";
import { SharedMaterialModule } from "app/shared/shared-material.module";
import { SharedModule } from "app/shared/shared.module";
import * as echarts from "echarts";
import { NgxEchartsModule } from "ngx-echarts";
import { ReportModule } from "../report/report.module";
import { AnalyticsComponent } from "./analytics/analytics.component";
import { DashboardLowStockReportComponent } from "./dashboard-low-stock-report/dashboard-low-stock-report.component";
import { DashboardRoutes } from "./dashboard.routing";

@NgModule({
  imports: [
    CommonModule,
    SharedMaterialModule,
    SharedModule,
    SharedComponentsModule,
    ReportModule,
    FlexLayoutModule,
    FormsModule,
    ReactiveFormsModule,
    SharedPipesModule,
    NgxEchartsModule.forRoot({
      echarts,
    }),
    RouterModule.forChild(DashboardRoutes),
  ],
  declarations: [AnalyticsComponent, DashboardLowStockReportComponent],
  exports: [],
})
export class DashboardModule {}
