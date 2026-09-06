import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, Resolve } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { PaymentVoucherService } from "../services/payment-voucher.service";

@Injectable({
  providedIn: "root",
})
export class PaymentVoucherResolverService implements Resolve<any> {
  constructor(private paymentVoucherService: PaymentVoucherService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.paymentVoucherService.getPaymentVoucherById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
