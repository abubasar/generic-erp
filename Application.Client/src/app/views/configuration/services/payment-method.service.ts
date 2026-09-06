import { Injectable } from "@angular/core";
import { PaymentMethodApiService } from "app/shared/api/configuration/payment-method-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { PaymentMethodRequest } from "../models/payment-method/payment-method-request.model";
import { PaymentMethod } from "../models/payment-method/payment-method.model";

@Injectable({
  providedIn: "root",
})
export class PaymentMethodService {
  constructor(private api: PaymentMethodApiService) {}

  getAllPaymentMethods(): Observable<SearchResponse<PaymentMethod>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<PaymentMethod>) => response));
  }
  getPaymentMethods(
    paymentMethodRequest: PaymentMethodRequest
  ): Observable<SearchResponse<PaymentMethod>> {
    return this.api
      .getAll(paymentMethodRequest)
      .pipe(map((response: SearchResponse<PaymentMethod>) => response));
  }

  createPaymentMethod(
    paymentMethod: PaymentMethod
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(paymentMethod)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updatePaymentMethod(
    paymentMethod: PaymentMethod
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(paymentMethod)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deletePaymentMethod(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
