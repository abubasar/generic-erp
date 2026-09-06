import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ReceivePaymentRequestDTO } from "app/views/sales/models/receive-payment/receive-payment-request-dto.model";
import { ReceivePaymentSearchRequestDTO } from "app/views/sales/models/receive-payment/receive-payment-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class ReceivePaymentApiService {
  baseURL = environment.apiURL + "/ReceivePayment";

  constructor(private httpClient: HttpClient) {}

  getAll(request: ReceivePaymentSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: ReceivePaymentSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(receivePayment: ReceivePaymentRequestDTO) {
    return this.httpClient.post(this.baseURL, receivePayment);
  }

  update(receivePayment: ReceivePaymentRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", receivePayment);
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

  sendToCustomer(id: string) {
    return this.httpClient.post(this.baseURL + `/send/${id}`, {});
  }

  unpost(id: string, status: number) {
    return this.httpClient.post(this.baseURL + `/unpost/${id}/${status}`, {});
  }
}
