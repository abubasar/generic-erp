import { Injectable } from "@angular/core";
import { PaymentModeApiService } from "app/shared/api/configuration/payment-mode-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { Observable, map } from "rxjs";
import { PaymentMode } from "../models/payment-mode/payment-mode.model";
import { PaymentModeRequest } from "../models/payment-mode/payment-mode-request.model";

@Injectable({
  providedIn: "root",
})
export class PaymentModeService {
  constructor(private api: PaymentModeApiService) {}

  getAllPaymentModes(): Observable<SearchResponse<PaymentMode>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<PaymentMode>) => response));
  }
  getPaymentModes(
    paymentModeRequest: PaymentModeRequest
  ): Observable<SearchResponse<PaymentMode>> {
    return this.api
      .getAll(paymentModeRequest)
      .pipe(map((response: SearchResponse<PaymentMode>) => response));
  }

  createPaymentMode(
    paymentMode: PaymentMode
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(paymentMode)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updatePaymentMode(
    paymentMode: PaymentMode
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(paymentMode)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deletePaymentMode(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
