import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { PurchaseInvoiceService } from "../services/purchase-invoice.service";

@Injectable({
  providedIn: "root",
})
export class PurchaseInvoiceResolverService  {
  constructor(private purchaseInvoiceService: PurchaseInvoiceService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.purchaseInvoiceService.getPurchaseInvoiceById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
