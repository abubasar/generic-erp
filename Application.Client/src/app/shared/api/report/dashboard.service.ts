import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class DashboardService {
  baseURL = environment.apiURL + "/dashboard";

  constructor(private httpClient: HttpClient) {}

  getDashboardStatistics(filterType: number) {
    return this.httpClient.get(this.baseURL + "/statistics/" + filterType);
  }
  getDashboardThisMonthSales() {
    return this.httpClient.get(this.baseURL + "/this-month-sale");
  }
  getDashboardLastMonthSales() {
    return this.httpClient.get(this.baseURL + "/last-month-sale");
  }
  getDashboardSalesFinancialYear(year: number) {
    return this.httpClient.get(this.baseURL + "/sales-financial-year/" + year);
  }
}
