import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Company } from "app/views/configuration/models/company/company.model";
import { CompanyRequest } from "app/views/configuration/models/company/company-request.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class CompanyApiService {
  baseURL = environment.apiURL + "/company";

  constructor(private httpClient: HttpClient) {}

  getAll(request: CompanyRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(company: Company) {
    return this.httpClient.post(this.baseURL, company);
  }

  update(company: Company) {
    return this.httpClient.post(this.baseURL + "/update", company);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
