import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { DiscountProductWiseByDateRequest } from "app/views/configuration/models/discount-product-wise/discount-product-wise-by-date-request-model";
import { DiscountProductWiseByProductIdRequest } from "app/views/configuration/models/discount-product-wise/discount-product-wise-by-product-id-request-model";
import { DiscountProductWiseRequest } from "app/views/configuration/models/discount-product-wise/discount-product-wise-request.model";
import { DiscountProductWise } from "app/views/configuration/models/discount-product-wise/discount-product-wise.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class DiscountProductWiseApiService {
  baseURL = environment.apiURL + "/DiscountProductWise";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/search", { page: -1 });
  }

  getAll(request: DiscountProductWiseRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(discountProductWise: DiscountProductWise) {
    return this.httpClient.post(this.baseURL, discountProductWise);
  }

  update(discountProductWise: DiscountProductWise) {
    return this.httpClient.post(this.baseURL + "/update", discountProductWise);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }

  discountPerKg(request: DiscountProductWiseByProductIdRequest) {
    return this.httpClient.post(this.baseURL + `/discount-per-kg`, request);
  }

  activeOfferDiscount(request: DiscountProductWiseByDateRequest) {
    return this.httpClient.post(this.baseURL + `/active-discount`, request);
  }
}
