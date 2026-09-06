import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ProductTypeRequest } from "app/views/configuration/models/product-type/product-type-request.model";
import { ProductType } from "app/views/configuration/models/product-type/product-type.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class ProductTypeApiService {
  baseURl = environment.apiURL + "/productType";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURl + "/search", { page: -1 });
  }
  getAll(request: ProductTypeRequest) {
    return this.httpClient.post(this.baseURl + "/search", request);
  }

  create(productType: ProductType) {
    return this.httpClient.post(this.baseURl, productType);
  }

  update(productType: ProductType) {
    return this.httpClient.post(this.baseURl + "/update", productType);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURl + `/delete/${id}`, {});
  }
}
