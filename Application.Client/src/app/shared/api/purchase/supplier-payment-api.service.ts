import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { SupplierPaymentRequestDTO } from "app/views/purchase/models/supplier-payment/supplier-payment-request-dto.model";
import { SupplierPaymentSearchRequestDTO } from "app/views/purchase/models/supplier-payment/supplier-payment-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class SupplierPaymentApiService {
  baseURL = environment.apiURL + "/SupplierPayment";

  constructor(private httpClient: HttpClient) {}

  getAll(request: SupplierPaymentSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: SupplierPaymentSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(supplierPayment: SupplierPaymentRequestDTO) {
    return this.httpClient.post(this.baseURL, supplierPayment);
  }

  update(supplierPayment: SupplierPaymentRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", supplierPayment);
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
