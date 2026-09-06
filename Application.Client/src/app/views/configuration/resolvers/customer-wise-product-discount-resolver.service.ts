import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, Resolve } from "@angular/router";
import { catchError, Observable, of } from "rxjs";
import { CustomerWiseProductDiscountService } from "../services/customer-wise-product-discount.service";

@Injectable({
  providedIn: "root",
})
export class CustomerWiseProductDiscountResolverService
  implements Resolve<any>
{
  constructor(
    private customerWiseProductDiscountService: CustomerWiseProductDiscountService
  ) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.customerWiseProductDiscountService
      .getCustomerWiseProductDiscountById(id)
      .pipe(
        catchError((error) => {
          return of("No data");
        })
      );
  }
}
