import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { EmailAccountRequest } from 'app/views/configuration/models/email-account/email-account-request.model';
import { EmailAccount } from 'app/views/configuration/models/email-account/email-account.model';
import { environment } from 'environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EmailAccountApiService {
  baseURL = environment.apiURL + "/EmailAccount";

  constructor(private httpClient: HttpClient) {}

  getAll(request: EmailAccountRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(emailAccount: EmailAccount) {
    return this.httpClient.post(this.baseURL, emailAccount);
  }

  update(emailAccount: EmailAccount) {
    return this.httpClient.post(this.baseURL + "/update", emailAccount);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
