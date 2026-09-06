import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CurrencyRequest } from "app/views/configuration/models/currency/currency-request.model";
import { Currency } from "app/views/configuration/models/currency/currency.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class CurrencyApiService {
  baseURL = environment.apiURL + "/currency";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/search", { page: -1 });
  }
  getAll(request: CurrencyRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(currency: Currency) {
    return this.httpClient.post(this.baseURL, currency);
  }

  update(currency: Currency) {
    return this.httpClient.post(this.baseURL + "/update", currency);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
