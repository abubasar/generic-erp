import { Injectable } from "@angular/core";
import { EmailAccountApiService } from "app/shared/api/configuration/email-account-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { EmailAccountRequest } from "../models/email-account/email-account-request.model";
import { EmailAccount } from "../models/email-account/email-account.model";

@Injectable({
  providedIn: "root",
})
export class EmailAccountService {
  constructor(private api: EmailAccountApiService) {}

  getEmailAccounts(
    emailAccountRequest: EmailAccountRequest
  ): Observable<SearchResponse<EmailAccount>> {
    return this.api
      .getAll(emailAccountRequest)
      .pipe(map((response: SearchResponse<EmailAccount>) => response));
  }

  createEmailAccount(
    emailAccount: EmailAccount
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(emailAccount)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateEmailAccount(
    emailAccount: EmailAccount
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(emailAccount)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteAccount(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
