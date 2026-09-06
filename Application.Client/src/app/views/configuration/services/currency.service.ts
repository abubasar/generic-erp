import { Injectable } from "@angular/core";
import { CurrencyApiService } from "app/shared/api/configuration/currency-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { CurrencyRequest } from "../models/currency/currency-request.model";
import { Currency } from "../models/currency/currency.model";

@Injectable({
  providedIn: "root",
})
export class CurrencyService {
  constructor(private api: CurrencyApiService) {}

  getAllCurrencies(): Observable<SearchResponse<Currency>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Currency>) => response));
  }
  getCurrencies(
    currencyRequest: CurrencyRequest
  ): Observable<SearchResponse<Currency>> {
    return this.api
      .getAll(currencyRequest)
      .pipe(map((response: SearchResponse<Currency>) => response));
  }

  createCurrency(currency: Currency): Observable<GeneralResponse<string>> {
    return this.api
      .create(currency)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateCurrency(currency: Currency): Observable<GeneralResponse<string>> {
    return this.api
      .update(currency)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteCurrency(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
