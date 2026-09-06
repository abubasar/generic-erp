import { Injectable } from "@angular/core";
import { CustomerWiseProductDiscountApiService } from "app/shared/api/configuration/customer-wise-product-discount-api.service";
import {
  GeneralResponse,
  GeneralResponse2,
} from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { Observable, map } from "rxjs";
import { CustomerInvoiceDiscount } from "../models/customer-wise-product-discount/customer-invoice-discount.model";
import { CustomerWiseProductDiscountByCustomerId } from "../models/customer-wise-product-discount/customer-wise-product-discount-by-customer-id.model";
import { CustomerWiseProductDiscountRequest } from "../models/customer-wise-product-discount/customer-wise-product-discount-request.model";
import { CustomerWiseProductDiscount } from "../models/customer-wise-product-discount/customer-wise-product-discount.model";

@Injectable({
  providedIn: "root",
})
export class CustomerWiseProductDiscountService {
  constructor(private api: CustomerWiseProductDiscountApiService) {}

  getAllCustomerWiseProductDiscounts(): Observable<
    SearchResponse<CustomerWiseProductDiscount>
  > {
    return this.api
      .getAlls()
      .pipe(
        map((response: SearchResponse<CustomerWiseProductDiscount>) => response)
      );
  }

  getCustomerWiseProductDiscountById(
    id: string
  ): Observable<GeneralResponse<CustomerWiseProductDiscount>> {
    return this.api
      .getById(id)
      .pipe(
        map(
          (response: GeneralResponse<CustomerWiseProductDiscount>) => response
        )
      );
  }

  getCustomerWiseProductDiscounts(
    customerWiseProductDiscountRequest: CustomerWiseProductDiscountRequest
  ): Observable<SearchResponse<CustomerWiseProductDiscount>> {
    return this.api
      .getAll(customerWiseProductDiscountRequest)
      .pipe(
        map((response: SearchResponse<CustomerWiseProductDiscount>) => response)
      );
  }

  createCustomerWiseProductDiscount(
    customerWiseProductDiscount: CustomerWiseProductDiscount
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(customerWiseProductDiscount)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateCustomerWiseProductDiscount(
    customerWiseProductDiscount: CustomerWiseProductDiscount
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(customerWiseProductDiscount)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteCustomerWiseProductDiscount(
    id: string
  ): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkCustomerWiseProductDiscount(
    id: string
  ): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveCustomerWiseProductDiscount(
    id: string
  ): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostCustomerWiseProductDiscount(
    id: string,
    status:number
  ): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  getCustomerWiseProductDiscountByProductId(
    customerId: string,
    productId: string
  ): Observable<GeneralResponse<CustomerInvoiceDiscount>> {
    return this.api
      .discount(customerId, productId)
      .pipe(
        map((response: GeneralResponse<CustomerInvoiceDiscount>) => response)
      );
  }

  getCustomerWiseProductDiscountByCustomerId(
    customerId: string
  ): Observable<GeneralResponse2<CustomerWiseProductDiscountByCustomerId>> {
    return this.api
      .activeCustomerDiscount(customerId)
      .pipe(
        map(
          (
            response: GeneralResponse2<CustomerWiseProductDiscountByCustomerId>
          ) => response
        )
      );
  }
}
