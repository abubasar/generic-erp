import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ReceiveVoucherRequestDTO } from "app/views/accounts/models/receive-voucher/receive-voucher-request-dto.model";
import { ReceiveVoucherSearchRequestDTO } from "app/views/accounts/models/receive-voucher/receive-voucher-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class ReceiveVoucherApiService {
  baseURL = environment.apiURL + "/ReceiveVoucher";

  constructor(private httpClient: HttpClient) {}

  getAll(request: ReceiveVoucherSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: ReceiveVoucherSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(receiveVoucher: ReceiveVoucherRequestDTO) {
    return this.httpClient.post(this.baseURL, receiveVoucher);
  }

  update(receiveVoucher: ReceiveVoucherRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", receiveVoucher);
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
