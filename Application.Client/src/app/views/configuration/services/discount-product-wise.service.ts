import { Injectable } from "@angular/core";
import { DiscountProductWiseApiService } from "app/shared/api/configuration/discount-product-wise-api.service";
import {
  GeneralResponse,
  GeneralResponse2,
} from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { Observable, map } from "rxjs";
import { ActiveDiscountProductWiseByDateResponse } from "../models/discount-product-wise/active-discount-product-wise-by-date-response.model";
import { DiscountProductWiseByProductIdRequest } from "../models/discount-product-wise/discount-product-wise-by-product-id-request-model";
import { DiscountProductWiseByProductIdResponse } from "../models/discount-product-wise/discount-product-wise-by-product-id-response.model";
import { DiscountProductWiseRequest } from "../models/discount-product-wise/discount-product-wise-request.model";
import { DiscountProductWise } from "../models/discount-product-wise/discount-product-wise.model";
import { DiscountProductWiseByDateRequest } from "../models/discount-product-wise/discount-product-wise-by-date-request-model";

@Injectable({
  providedIn: "root",
})
export class DiscountProductWiseService {
  constructor(private api: DiscountProductWiseApiService) {}

  getAllDiscountProductWises(): Observable<
    SearchResponse<DiscountProductWise>
  > {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<DiscountProductWise>) => response));
  }
  getDiscountProductWises(
    discountProductWiseRequest: DiscountProductWiseRequest
  ): Observable<SearchResponse<DiscountProductWise>> {
    return this.api
      .getAll(discountProductWiseRequest)
      .pipe(map((response: SearchResponse<DiscountProductWise>) => response));
  }

  createDiscountProductWise(
    discountProductWise: DiscountProductWise
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(discountProductWise)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateDiscountProductWise(
    discountProductWise: DiscountProductWise
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(discountProductWise)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteDiscountProductWise(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  getDiscountProductWiseByProductId(
    request: DiscountProductWiseByProductIdRequest
  ): Observable<GeneralResponse<DiscountProductWiseByProductIdResponse>> {
    return this.api
      .discountPerKg(request)
      .pipe(
        map(
          (response: GeneralResponse<DiscountProductWiseByProductIdResponse>) =>
            response
        )
      );
  }

  getActiveDiscountProductWiseByDate(
    request: DiscountProductWiseByDateRequest
  ): Observable<GeneralResponse2<ActiveDiscountProductWiseByDateResponse>> {
    return this.api
      .activeOfferDiscount(request)
      .pipe(
        map(
          (
            response: GeneralResponse2<ActiveDiscountProductWiseByDateResponse>
          ) => response
        )
      );
  }
}
