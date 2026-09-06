import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CustomerWiseProductDiscountRequest } from "app/views/configuration/models/customer-wise-product-discount/customer-wise-product-discount-request.model";
import { CustomerWiseProductDiscount } from "app/views/configuration/models/customer-wise-product-discount/customer-wise-product-discount.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class CustomerWiseProductDiscountApiService {
  baseURL = environment.apiURL + "/CustomerWiseProductDiscount";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/search", { page: -1 });
  }

  getAll(request: CustomerWiseProductDiscountRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  create(customerWiseProductDiscount: CustomerWiseProductDiscount) {
    return this.httpClient.post(this.baseURL, customerWiseProductDiscount);
  }

  update(customerWiseProductDiscount: CustomerWiseProductDiscount) {
    return this.httpClient.post(
      this.baseURL + "/update",
      customerWiseProductDiscount
    );
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }

  check(id: string) {
    return this.httpClient.post(this.baseURL + `/check/${id}`, {});
  }

  approve(id: string) {
    return this.httpClient.post(this.baseURL + `/approve/${id}`, {});
  }

  unpost(id: string, status: number) {
    return this.httpClient.post(this.baseURL + `/unpost/${id}/${status}`, {});
  }

  discount(customerId: string, productId: string) {
    return this.httpClient.get(
      this.baseURL + `/discount/${customerId}/${productId}`
    );
  }

  activeCustomerDiscount(customerId: string) {
    return this.httpClient.get(this.baseURL + `/active-discount/${customerId}`);
  }
}
