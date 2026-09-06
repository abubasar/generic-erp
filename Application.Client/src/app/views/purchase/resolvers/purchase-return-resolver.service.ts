import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, Resolve } from "@angular/router";
import { PurchaseReturnService } from "../services/purchase-return.service";
import { Observable, catchError, of } from "rxjs";

@Injectable({
  providedIn: "root",
})
export class PurchaseReturnResolverService implements Resolve<any> {
  constructor(private purchaseReturnService: PurchaseReturnService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    console.log("Called Get Purchase Return By Id in resolver...", route, id);
    return this.purchaseReturnService.getPurchaseReturnById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
