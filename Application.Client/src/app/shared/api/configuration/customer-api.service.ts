import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CustomerRequest } from "app/views/configuration/models/customer/customer-request.model";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class CustomerApiService {
  baseURL = environment.apiURL + "/customer";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/all", {});
  }
  getAll(request: CustomerRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(customer: Customer) {
    return this.httpClient.post(this.baseURL, customer);
  }

  update(customer: Customer) {
    return this.httpClient.post(this.baseURL + "/update", customer);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }

  creditLimitBalance(customerId: string) {
    return this.httpClient.get(
      this.baseURL + `/get/customer-credit-limit-balance/${customerId}`
    );
  }

  customerBalance(customerId: string) {
    return this.httpClient.get(
      this.baseURL + `/get/customer-balance/${customerId}`
    );
  }
}
