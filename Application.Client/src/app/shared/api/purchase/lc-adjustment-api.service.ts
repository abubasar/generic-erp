import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { LcAdjustmentRequestDTO } from "app/views/purchase/models/lc-adjustment/lc-adjustment-request-dto.model";
import { LcAdjustmentSearchRequestDTO } from "app/views/purchase/models/lc-adjustment/lc-adjustment-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class LcAdjustmentApiService {
  baseURL = environment.apiURL + "/LcAdjustment";

  constructor(private httpClient: HttpClient) {}

  getAll(request: LcAdjustmentSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: LcAdjustmentSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(lcAdjustment: LcAdjustmentRequestDTO) {
    return this.httpClient.post(this.baseURL, lcAdjustment);
  }

  update(lcAdjustment: LcAdjustmentRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", lcAdjustment);
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
