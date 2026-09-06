import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { PaymentVoucherRequestDTO } from "app/views/accounts/models/payment-voucher/payment-voucher-request-dto.model";
import { PaymentVoucherSearchRequestDTO } from "app/views/accounts/models/payment-voucher/payment-voucher-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class PaymentVoucherApiService {
  baseURL = environment.apiURL + "/PaymentVoucher";

  constructor(private httpClient: HttpClient) {}

  getAll(request: PaymentVoucherSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: PaymentVoucherSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(paymentVoucher: PaymentVoucherRequestDTO) {
    return this.httpClient.post(this.baseURL, paymentVoucher);
  }

  update(paymentVoucher: PaymentVoucherRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", paymentVoucher);
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
