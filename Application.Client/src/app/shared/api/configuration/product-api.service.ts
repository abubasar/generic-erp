import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ProductRequest } from "app/views/configuration/models/product/product-request.model";
import { ProductDTO } from "app/views/configuration/models/product/product-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class ProductApiService {
  baseURL = environment.apiURL + "/product";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/all", {});
  }
  getAll(request: ProductRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(product: ProductDTO) {
    return this.httpClient.post(this.baseURL, product);
  }

  update(product: ProductDTO) {
    return this.httpClient.post(this.baseURL + "/update", product);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
