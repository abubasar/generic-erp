import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class AccountReportApiService {
  baseURL = environment.apiURL + "/account";

  constructor(private httpClient: HttpClient) {}

  getChartOfAccounts() {
    return this.httpClient.get(this.baseURL + "/chart-of-accounts");
  }
}
