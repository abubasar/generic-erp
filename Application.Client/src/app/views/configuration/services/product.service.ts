import { Injectable } from "@angular/core";
import { ProductApiService } from "app/shared/api/configuration/product-api.service";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { ProductRequest } from "../models/product/product-request.model";

import { ProductDTO } from "../models/product/product-dto.model";
import { ProductView } from "../models/product/product-view.model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";

@Injectable({
  providedIn: "root",
})
export class ProductService {
  constructor(private api: ProductApiService) {}

  getAllProducts(): Observable<SearchResponse<ProductView>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<ProductView>) => response));
  }
  getProducts(
    productRequest: ProductRequest
  ): Observable<SearchResponse<ProductView>> {
    return this.api
      .getAll(productRequest)
      .pipe(map((response: SearchResponse<ProductView>) => response));
  }

  createProduct(product: ProductDTO): Observable<GeneralResponse<string>> {
    return this.api
      .create(product)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateProduct(product: ProductDTO): Observable<GeneralResponse<string>> {
    return this.api
      .update(product)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteProduct(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
