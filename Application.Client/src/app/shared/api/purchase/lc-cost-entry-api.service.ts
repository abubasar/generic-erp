import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { LCCostEntryRequestDTO } from "app/views/purchase/models/lc-cost-entry/lc-cost-entry-request-dto.model";
import { LCCostEntrySearchRequestDTO } from "app/views/purchase/models/lc-cost-entry/lc-cost-entry-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class LcCostEntryApiService {
  baseURL = environment.apiURL + "/LCCostEntry";

  constructor(private httpClient: HttpClient) {}

  getAll(request: LCCostEntrySearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: LCCostEntrySearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getLcCostEntryByPoId(purchaseOrderId: string) {
    return this.httpClient.get(
      this.baseURL + `/lc-cost-entry-by-poId/${purchaseOrderId}`,
      {}
    );
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(lcCostEntry: LCCostEntryRequestDTO) {
    return this.httpClient.post(this.baseURL, lcCostEntry);
  }

  update(lcCostEntry: LCCostEntryRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", lcCostEntry);
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
