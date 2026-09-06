import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { PaymentModeRequest } from 'app/views/configuration/models/payment-mode/payment-mode-request.model';
import { PaymentMode } from 'app/views/configuration/models/payment-mode/payment-mode.model';
import { environment } from 'environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PaymentModeApiService {
  baseURL = environment.apiURL + "/paymentMode";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/all", {});
  }
  getAll(request: PaymentModeRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(paymentMode: PaymentMode) {
    return this.httpClient.post(this.baseURL, paymentMode);
  }

  update(paymentMode: PaymentMode) {
    return this.httpClient.post(this.baseURL + "/update", paymentMode);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
