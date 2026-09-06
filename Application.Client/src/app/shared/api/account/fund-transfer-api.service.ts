import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { FundTransferRequestDTO } from "app/views/accounts/models/fund-transfer/fund-transfer-request-dto.model";
import { FundTransferSearchRequestDTO } from "app/views/accounts/models/fund-transfer/fund-transfer-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class FundTransferApiService {
  baseURL = environment.apiURL + "/FundTransfer";

  constructor(private httpClient: HttpClient) {}

  getAll(request: FundTransferSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: FundTransferSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(fundTransfer: FundTransferRequestDTO) {
    return this.httpClient.post(this.baseURL, fundTransfer);
  }

  update(fundTransfer: FundTransferRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", fundTransfer);
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
