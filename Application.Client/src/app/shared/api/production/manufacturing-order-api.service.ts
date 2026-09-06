import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ManufacturingOrderRequestDTO } from "app/views/production/models/manufacturing-order/manufacturing-order-request-dto.model";
import { ManufacturingOrderSearchRequestDTO } from "app/views/production/models/manufacturing-order/manufacturing-order-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class ManufacturingOrderApiService {
  baseURL = environment.apiURL + "/manufacturingOrder";

  constructor(private httpClient: HttpClient) {}

  getAll(request: ManufacturingOrderSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: ManufacturingOrderSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(manufacturingOrder: ManufacturingOrderRequestDTO) {
    return this.httpClient.post(this.baseURL, manufacturingOrder);
  }

  update(manufacturingOrder: ManufacturingOrderRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", manufacturingOrder);
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

  unpost(id: string, status: number) {
    return this.httpClient.post(this.baseURL + `/unpost/${id}/${status}`, {});
  }
}
