import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { AccountTypeRequest } from "app/views/configuration/models/account-type/account-type-request.model";
import { AccountType } from "app/views/configuration/models/account-type/account-type.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class AccountTypeApiService {
  baseURL = environment.apiURL + "/accountType";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/all", {});
  }
  getAll(request: AccountTypeRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(accountType: AccountType) {
    return this.httpClient.post(this.baseURL, accountType);
  }

  update(accountType: AccountType) {
    return this.httpClient.post(this.baseURL + "/update", accountType);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
