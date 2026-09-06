import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CountryRequest } from "app/views/configuration/models/country/country-request.model";
import { Country } from "app/views/configuration/models/country/country.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class CountryApiService {
  baseURL = environment.apiURL + "/country";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/all", {});
  }

  getAll(request: CountryRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(country: Country) {
    return this.httpClient.post(this.baseURL, country);
  }

  update(country: Country) {
    return this.httpClient.post(this.baseURL + "/update", country);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
