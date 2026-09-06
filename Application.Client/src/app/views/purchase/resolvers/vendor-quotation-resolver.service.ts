import { Injectable } from "@angular/core";
import { Resolve, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { VendorQuotationService } from "../services/vendor-quotation.service";

@Injectable({
  providedIn: "root",
})
export class VendorQuotationResolverService implements Resolve<any> {
  constructor(private vendorQuotationService: VendorQuotationService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.vendorQuotationService.getVendorQuotationById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
