import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { ReceivePaymentService } from "../services/receive-payment.service";

@Injectable({
  providedIn: "root",
})
export class ReceivePaymentResolverService  {
  constructor(private receivePaymentService: ReceivePaymentService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.receivePaymentService.getReceivePaymentById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
