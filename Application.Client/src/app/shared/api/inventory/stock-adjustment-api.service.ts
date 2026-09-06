import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { StockAdjustmentRequestDTO } from "app/views/inventory/models/stock-adjustment/stock-adjustment-request-dto.model";
import { StockAdjustmentSearchRequestDTO } from "app/views/inventory/models/stock-adjustment/stock-adjustment-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class StockAdjustmentApiService {
  baseURL = environment.apiURL + "/stockAdjustment";

  constructor(private httpClient: HttpClient) {}

  getAll(request: StockAdjustmentSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(stockAdjustment: StockAdjustmentRequestDTO) {
    return this.httpClient.post(this.baseURL, stockAdjustment);
  }

  update(stockAdjustment: StockAdjustmentRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", stockAdjustment);
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
