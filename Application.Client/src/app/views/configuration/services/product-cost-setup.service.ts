import { Injectable } from "@angular/core";
import { ProductCostSetupApiService } from "app/shared/api/configuration/product-cost-setup-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { ProductCostSetupRequest } from "../models/product-cost-setup/product-cost-setup-request.model";
import { ProductCostSetup } from "../models/product-cost-setup/product-cost-setup.model";

@Injectable({
  providedIn: "root",
})
export class ProductCostSetupService {
  constructor(private api: ProductCostSetupApiService) {}

  getProductCostSetups(
    productCostSetupRequest: ProductCostSetupRequest
  ): Observable<SearchResponse<ProductCostSetup>> {
    return this.api
      .getAll(productCostSetupRequest)
      .pipe(map((response: SearchResponse<ProductCostSetup>) => response));
  }

  createProductCostSetup(
    productCostSetup: ProductCostSetup
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(productCostSetup)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateProductCostSetup(
    productCostSetup: ProductCostSetup
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(productCostSetup)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteProductCostSetup(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
