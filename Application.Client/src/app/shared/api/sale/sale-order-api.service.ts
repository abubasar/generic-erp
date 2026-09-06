import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { SaleOrderRequestDTO } from "app/views/sales/models/sale-order/sale-order-request-dto.model";
import { SaleOrderSearchRequestDTO } from "app/views/sales/models/sale-order/sale-order-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class SaleOrderApiService {
  baseURL = environment.apiURL + "/SaleOrder";

  constructor(private httpClient: HttpClient) {}

  getAll(request: SaleOrderSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: SaleOrderSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(saleOrder: SaleOrderRequestDTO) {
    return this.httpClient.post(this.baseURL, saleOrder);
  }

  update(saleOrder: SaleOrderRequestDTO, headers: HttpHeaders) {
    return this.httpClient.post(this.baseURL + "/update", saleOrder, {
      headers: headers,
    });
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

  close(id: string) {
    return this.httpClient.post(this.baseURL + `/close/${id}`, {});
  }

  unpost(id: string, status: number) {
    return this.httpClient.post(this.baseURL + `/unpost/${id}/${status}`, {});
  }
}
