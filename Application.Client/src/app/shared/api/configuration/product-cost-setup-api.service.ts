import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ProductCostSetupRequest } from "app/views/configuration/models/product-cost-setup/product-cost-setup-request.model";
import { ProductCostSetup } from "app/views/configuration/models/product-cost-setup/product-cost-setup.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class ProductCostSetupApiService {
  baseURL = environment.apiURL + "/ProductCostSetup";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/search", { page: -1 });
  }

  getAll(request: ProductCostSetupRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(productCostSetup: ProductCostSetup) {
    return this.httpClient.post(this.baseURL, productCostSetup);
  }

  update(productCostSetup: ProductCostSetup) {
    return this.httpClient.post(this.baseURL + "/update", productCostSetup);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
