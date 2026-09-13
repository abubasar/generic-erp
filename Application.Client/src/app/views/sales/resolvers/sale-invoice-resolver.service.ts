import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { SaleInvoiceService } from "../services/sale-invoice.service";

@Injectable({
  providedIn: "root",
})
export class SaleInvoiceResolverService  {
  constructor(private saleInvoiceService: SaleInvoiceService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.saleInvoiceService.getSaleInvoiceById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
