import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { VoucherEntryRequestDTO } from "app/views/accounts/models/voucher-entry/voucher-entry-request-dto.model";
import { VoucherEntrySearchRequestDTO } from "app/views/accounts/models/voucher-entry/voucher-entry-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class VoucherEntryApiService {
  baseURL = environment.apiURL + "/VoucherEntry";

  constructor(private httpClient: HttpClient) {}

  getAll(request: VoucherEntrySearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(voucherEntry: VoucherEntryRequestDTO) {
    return this.httpClient.post(this.baseURL, voucherEntry);
  }

  update(voucherEntry: VoucherEntryRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", voucherEntry);
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
