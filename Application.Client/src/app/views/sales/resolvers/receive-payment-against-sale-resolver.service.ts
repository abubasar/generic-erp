import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { ReceivePaymentAgainstSaleService } from "../services/receive-payment-against-sale.service";

@Injectable({
  providedIn: "root",
})
export class ReceivePaymentAgainstSaleResolverService {
  constructor(
    private receivePaymentAgainstSaleService: ReceivePaymentAgainstSaleService
  ) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.receivePaymentAgainstSaleService
      .getReceivePaymentAgainstSaleById(id)
      .pipe(
        catchError((error) => {
          return of("No data");
        })
      );
  }
}
