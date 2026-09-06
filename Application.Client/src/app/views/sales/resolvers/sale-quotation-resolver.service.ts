import { Injectable } from "@angular/core";
import { Resolve, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { SaleQuotationService } from "../services/sale-quotation.service";

@Injectable({
  providedIn: "root",
})
export class SaleQuotationResolverService implements Resolve<any> {
  constructor(private saleQuotationService: SaleQuotationService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    console.log("Called Get Sale Return in resolver...", route, id);
    return this.saleQuotationService.getSaleQuotationById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
