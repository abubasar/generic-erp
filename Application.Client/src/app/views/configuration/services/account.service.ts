import { Injectable } from "@angular/core";
import { AccountApiService } from "app/shared/api/configuration/account-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { AccountRequest } from "../models/account/account-request.model";
import { Account } from "../models/account/account.model";
import { ParentAccount } from "../models/account/parent-account.model";
import { ControlAccount } from "../models/account/control-account.model";

@Injectable({
  providedIn: "root",
})
export class AccountService {
  constructor(private api: AccountApiService) {}

  getAllAccounts(): Observable<SearchResponse<Account>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Account>) => response));
  }

  getAccounts(
    accountRequest: AccountRequest
  ): Observable<SearchResponse<Account>> {
    return this.api
      .getAll(accountRequest)
      .pipe(map((response: SearchResponse<Account>) => response));
  }

  createAccount(account: Account): Observable<GeneralResponse<string>> {
    return this.api
      .create(account)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateAccount(account: Account): Observable<GeneralResponse<string>> {
    return this.api
      .update(account)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteAccount(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  getParentAccounts(): Observable<GeneralResponse<ParentAccount[]>> {
    return this.api
      .getAllParentAccounts()
      .pipe(map((response: GeneralResponse<ParentAccount[]>) => response));
  }

  getAllControlAccounts(): Observable<GeneralResponse<ControlAccount[]>> {
    return this.api
      .getAllControlAccounts()
      .pipe(map((response: GeneralResponse<ControlAccount[]>) => response));
  }
  getAllControlAccountsExcludingCustomers(): Observable<
    GeneralResponse<ControlAccount[]>
  > {
    return this.api
      .getAllControlAccountsExcludingCustomers()
      .pipe(map((response: GeneralResponse<ControlAccount[]>) => response));
  }
  getAllControlAccountsExcludingSuppliers(): Observable<
    GeneralResponse<ControlAccount[]>
  > {
    return this.api
      .getAllControlAccountsExcludingSuppliers()
      .pipe(map((response: GeneralResponse<ControlAccount[]>) => response));
  }

  getControlAccountsByParentId(
    parentId: string
  ): Observable<GeneralResponse<ControlAccount[]>> {
    return this.api
      .getAllControlAccountsByParentId(parentId)
      .pipe(map((response: GeneralResponse<ControlAccount[]>) => response));
  }
}
