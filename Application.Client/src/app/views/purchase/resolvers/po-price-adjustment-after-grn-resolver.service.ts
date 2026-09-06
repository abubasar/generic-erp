import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, Resolve } from "@angular/router";
import { PoPriceAdjustmentAfterGrnService } from "../services/po-price-adjustment-after-grn.service";
import { Observable, catchError, of } from "rxjs";

@Injectable({
  providedIn: "root",
})
export class PoPriceAdjustmentAfterGrnResolverService implements Resolve<any> {
  constructor(
    private poPriceAdjustmentAfterGrnService: PoPriceAdjustmentAfterGrnService
  ) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    // console.log(
    //   "Called Get PO Price Adjustment After Grn By Id in resolver...",
    //   route,
    //   id
    // );
    return this.poPriceAdjustmentAfterGrnService
      .getPoPriceAdjustmentAfterGrnById(id)
      .pipe(
        catchError((error) => {
          return of("No data");
        })
      );
  }
}
