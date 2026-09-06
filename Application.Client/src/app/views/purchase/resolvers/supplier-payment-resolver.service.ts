import { Injectable } from "@angular/core";
import { Resolve, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { SupplierPaymentService } from "../services/supplier-payment.service";

@Injectable({
  providedIn: "root",
})
export class SupplierPaymentResolverService implements Resolve<any> {
  constructor(private supplierPaymentService: SupplierPaymentService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.supplierPaymentService.getSupplierPaymentById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
