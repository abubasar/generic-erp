import { Injectable } from "@angular/core";
import { FinancialYearApiService } from "app/shared/api/configuration/financial-year-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { FinancialYearRequest } from "../models/financial-year/financial-year-request.model";
import { FinancialYear } from "../models/financial-year/financial-year.model";

@Injectable({
  providedIn: "root",
})
export class FinancialYearService {
  constructor(private api: FinancialYearApiService) {}

  getAllFinancialYears(): Observable<SearchResponse<FinancialYear>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<FinancialYear>) => response));
  }
  getFinancialYears(
    financialYearRequest: FinancialYearRequest
  ): Observable<SearchResponse<FinancialYear>> {
    return this.api
      .getAll(financialYearRequest)
      .pipe(map((response: SearchResponse<FinancialYear>) => response));
  }

  createFinancialYear(
    financialYear: FinancialYear
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(financialYear)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateFinancialYear(
    financialYear: FinancialYear
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(financialYear)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteFinancialYear(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
