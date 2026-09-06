import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { AccountRequest } from "app/views/configuration/models/account/account-request.model";
import { Account } from "app/views/configuration/models/account/account.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class AccountApiService {
  baseURL = environment.apiURL + "/account";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/all",{});
  }

  getAll(request: AccountRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(account: Account) {
    return this.httpClient.post(this.baseURL, account);
  }

  update(account: Account) {
    return this.httpClient.post(this.baseURL + "/update", account);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }

  getAllParentAccounts() {
    return this.httpClient.get(this.baseURL + "/parent-accounts");
  }

  getAllControlAccounts() {
    return this.httpClient.get(this.baseURL + `/control-accounts`);
  }
  getAllControlAccountsExcludingCustomers() {
    return this.httpClient.get(
      this.baseURL + `/control-accounts-excluding-customers`
    );
  }
  getAllControlAccountsExcludingSuppliers() {
    return this.httpClient.get(
      this.baseURL + `/control-accounts-excluding-suppliers`
    );
  }
  getAllControlAccountsByParentId(parentId: string) {
    return this.httpClient.get(
      this.baseURL + `/getControlAccounts/${parentId}`
    );
  }
}
