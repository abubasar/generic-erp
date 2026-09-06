import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { FinancialYearRequest } from "app/views/configuration/models/financial-year/financial-year-request.model";
import { FinancialYear } from "app/views/configuration/models/financial-year/financial-year.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class FinancialYearApiService {
  baseURL = environment.apiURL + "/financialYear";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/search", { page: -1 });
  }
  getAll(request: FinancialYearRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(financialYear: FinancialYear) {
    return this.httpClient.post(this.baseURL, financialYear);
  }

  update(financialYear: FinancialYear) {
    return this.httpClient.post(this.baseURL + "/update", financialYear);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
