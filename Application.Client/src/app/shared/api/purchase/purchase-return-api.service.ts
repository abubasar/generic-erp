import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { PurchaseReturnRequestDTO } from "app/views/purchase/models/purchase-return/purchase-return-request-dto.model";
import { PurchaseReturnSearchRequestDTO } from "app/views/purchase/models/purchase-return/purchase-return-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class PurchaseReturnApiService {
  baseURL = environment.apiURL + "/PurchaseReturn";

  constructor(private httpClient: HttpClient) {}

  getAll(request: PurchaseReturnSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: PurchaseReturnSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(saleReturn: PurchaseReturnRequestDTO) {
    return this.httpClient.post(this.baseURL, saleReturn);
  }

  update(saleReturn: PurchaseReturnRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", saleReturn);
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
