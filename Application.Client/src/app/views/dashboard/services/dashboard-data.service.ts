import { Injectable } from "@angular/core";
import { DashboardService } from "app/shared/api/report/dashboard.service";
import {
  GeneralResponse,
  GeneralResponse2,
} from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { Observable, map } from "rxjs";
import { DashboardStatisticsViewModel } from "../models/dashboard-statistics-view-model";
import { DashboardThisMonthSaleViewModel } from "../models/dashboard-this-month-sale-view-model";
import { DashboardLastMonthSaleViewModel } from "../models/dashboard-last-month-sale-view-model";
import { DashboardSalesFinancialYearViewModel } from "../models/dashboard-sales-financial-year-view-model";

@Injectable({
  providedIn: "root",
})
export class DashboardDataService {
  constructor(private api: DashboardService) {}

  getDashboardStatistics(
    filterType: number
  ): Observable<GeneralResponse<DashboardStatisticsViewModel>> {
    return this.api
      .getDashboardStatistics(filterType)
      .pipe(
        map(
          (response: GeneralResponse<DashboardStatisticsViewModel>) => response
        )
      );
  }
  getDashboardThisMonthSales(): Observable<
    GeneralResponse2<DashboardThisMonthSaleViewModel>
  > {
    return this.api
      .getDashboardThisMonthSales()
      .pipe(
        map(
          (response: GeneralResponse2<DashboardThisMonthSaleViewModel>) =>
            response
        )
      );
  }
  getDashboardLastMonthSales(): Observable<
    GeneralResponse2<DashboardLastMonthSaleViewModel>
  > {
    return this.api
      .getDashboardLastMonthSales()
      .pipe(
        map(
          (response: GeneralResponse2<DashboardLastMonthSaleViewModel>) =>
            response
        )
      );
  }
  getDashboardSalesFinancialYear(
    year: number
  ): Observable<GeneralResponse2<DashboardSalesFinancialYearViewModel>> {
    return this.api
      .getDashboardSalesFinancialYear(year)
      .pipe(
        map(
          (response: GeneralResponse2<DashboardSalesFinancialYearViewModel>) => response
        )
      );
  }
}
