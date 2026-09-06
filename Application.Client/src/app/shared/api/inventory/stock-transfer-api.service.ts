import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { StockTransferRequestDTO } from "app/views/inventory/models/stock-transfer/stock-transfer-request-dto.model";
import { StockTransferSearchRequestDTO } from "app/views/inventory/models/stock-transfer/stock-transfer-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class StockTransferApiService {
  baseURL = environment.apiURL + "/StockTransfer";

  constructor(private httpClient: HttpClient) {}

  getAll(request: StockTransferSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(stockTransfer: StockTransferRequestDTO) {
    return this.httpClient.post(this.baseURL, stockTransfer);
  }

  update(stockTransfer: StockTransferRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", stockTransfer);
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
