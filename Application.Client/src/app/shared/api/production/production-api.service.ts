import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ProductionRequestDTO } from "app/views/production/models/production/production-request-dto.model";
import { ProductionSearchRequestDTO } from "app/views/production/models/production/production-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class ProductionApiService {
  baseURL = environment.apiURL + "/production";

  constructor(private httpClient: HttpClient) {}

  getAll(request: ProductionSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: ProductionSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(production: ProductionRequestDTO) {
    return this.httpClient.post(this.baseURL, production);
  }

  update(production: ProductionRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", production);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }

  check(id: string) {
    return this.httpClient.post(this.baseURL + `/check/${id}`, {});
  }

  approve(id: string) {
    return this.httpClient.post(this.baseURL + `/approve/${id}`, {});
  }

  unpost(id: string,status:number) {
    return this.httpClient.post(this.baseURL + `/unpost/${id}/${status}`, {});
  }
}
