import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { PaymentMethodRequest } from "app/views/configuration/models/payment-method/payment-method-request.model";
import { PaymentMethod } from "app/views/configuration/models/payment-method/payment-method.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class PaymentMethodApiService {
  baseURL = environment.apiURL + "/paymentMethod";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/all", {});
  }
  getAll(request: PaymentMethodRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(paymentMethod: PaymentMethod) {
    return this.httpClient.post(this.baseURL, paymentMethod);
  }

  update(paymentMethod: PaymentMethod) {
    return this.httpClient.post(this.baseURL + "/update", paymentMethod);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
