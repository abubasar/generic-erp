import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ReceivePaymentAgainstSaleRequestDTO } from "app/views/sales/models/receive-payment-against-sale/receive-payment-against-sale-request-dto.model";
import { ReceivePaymentAgainstSaleSearchRequestDTO } from "app/views/sales/models/receive-payment-against-sale/receive-payment-against-sale-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class ReceivePaymentAgainstSaleApiService {
  baseURL = environment.apiURL + "/ReceivePaymentAgainstSale";

  constructor(private httpClient: HttpClient) {}

  getAll(request: ReceivePaymentAgainstSaleSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: ReceivePaymentAgainstSaleSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(receivePaymentAgainstSale: ReceivePaymentAgainstSaleRequestDTO) {
    return this.httpClient.post(this.baseURL, receivePaymentAgainstSale);
  }

  update(receivePaymentAgainstSale: ReceivePaymentAgainstSaleRequestDTO) {
    return this.httpClient.post(
      this.baseURL + "/update",
      receivePaymentAgainstSale
    );
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
