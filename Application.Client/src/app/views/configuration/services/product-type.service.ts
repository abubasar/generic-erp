import { Injectable } from "@angular/core";
import { ProductTypeApiService } from "app/shared/api/configuration/product-type-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { ProductTypeRequest } from "../models/product-type/product-type-request.model";
import { ProductType } from "../models/product-type/product-type.model";

@Injectable({
  providedIn: "root",
})
export class ProductTypeService {
  constructor(private api: ProductTypeApiService) {}

  getAllProductTypes(): Observable<SearchResponse<ProductType>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<ProductType>) => response));
  }
  getProductTypes(
    productTypeRequest: ProductTypeRequest
  ): Observable<SearchResponse<ProductType>> {
    return this.api
      .getAll(productTypeRequest)
      .pipe(map((response: SearchResponse<ProductType>) => response));
  }

  createProductType(
    productType: ProductType
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(productType)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateProductType(
    productType: ProductType
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(productType)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteProductType(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
