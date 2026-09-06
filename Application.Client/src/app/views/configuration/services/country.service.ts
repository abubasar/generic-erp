import { Injectable } from "@angular/core";
import { CountryApiService } from "app/shared/api/configuration/country-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { CountryRequest } from "../models/country/country-request.model";
import { Country } from "../models/country/country.model";

@Injectable({
  providedIn: "root",
})
export class CountryService {
  constructor(private api: CountryApiService) {}

  getAllCountries(): Observable<SearchResponse<Country>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Country>) => response));
  }

  getCountries(
    countryRequest: CountryRequest
  ): Observable<SearchResponse<Country>> {
    return this.api
      .getAll(countryRequest)
      .pipe(map((response: SearchResponse<Country>) => response));
  }

  createCountry(country: Country): Observable<GeneralResponse<string>> {
    return this.api
      .create(country)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateCountry(country: Country): Observable<GeneralResponse<string>> {
    return this.api
      .update(country)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteCountry(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
