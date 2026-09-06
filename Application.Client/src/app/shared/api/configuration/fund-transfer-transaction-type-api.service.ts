import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { FundTransferTransactionTypeRequest } from "app/views/configuration/models/fund-transfer-transaction-type/fund-transfer-transaction-type-request.model";
import { FundTransferTransactionType } from "app/views/configuration/models/fund-transfer-transaction-type/fund-transfer-transaction-type.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class FundTransferTransactionTypeApiService {
  baseURl = environment.apiURL + "/FundTransferTransactionType";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURl + "/search", { page: -1 });
  }
  getAll(request: FundTransferTransactionTypeRequest) {
    return this.httpClient.post(this.baseURl + "/search", request);
  }

  create(fundTransferTransactionType: FundTransferTransactionType) {
    return this.httpClient.post(this.baseURl, fundTransferTransactionType);
  }

  update(fundTransferTransactionType: FundTransferTransactionType) {
    return this.httpClient.post(
      this.baseURl + "/update",
      fundTransferTransactionType
    );
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURl + `/delete/${id}`, {});
  }
}
