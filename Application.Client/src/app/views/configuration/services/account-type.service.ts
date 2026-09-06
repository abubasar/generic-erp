import { Injectable } from "@angular/core";
import { AccountTypeApiService } from "app/shared/api/configuration/account-type-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { AccountTypeRequest } from "../models/account-type/account-type-request.model";
import { AccountType } from "../models/account-type/account-type.model";

@Injectable({
  providedIn: "root",
})
export class AccountTypeService {
  constructor(private api: AccountTypeApiService) {}
  getAllAccountTypes(
  ): Observable<SearchResponse<AccountType>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<AccountType>) => response));
  }
  getAccountTypes(
    accountTypeRequest: AccountTypeRequest
  ): Observable<SearchResponse<AccountType>> {
    return this.api
      .getAll(accountTypeRequest)
      .pipe(map((response: SearchResponse<AccountType>) => response));
  }

  createAccountType(
    accountType: AccountType
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(accountType)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateAccountType(
    accountType: AccountType
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(accountType)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteAccountType(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
