import { Injectable } from "@angular/core";
import { CompanyApiService } from "app/shared/api/configuration/company-api.service";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { Company } from "../models/company/company.model";
import { CompanyRequest } from "../models/company/company-request.model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";

@Injectable({
  providedIn: "root",
})
export class CompanyService {
  constructor(private api: CompanyApiService) {}

  getCompanies(
    companyRequest: CompanyRequest
  ): Observable<SearchResponse<Company>> {
    return this.api
      .getAll(companyRequest)
      .pipe(map((response: SearchResponse<Company>) => response));
  }

  createCompany(company: Company): Observable<GeneralResponse<string>> {
    return this.api
      .create(company)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateCompany(company: Company): Observable<GeneralResponse<string>> {
    return this.api
      .update(company)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteCompany(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
