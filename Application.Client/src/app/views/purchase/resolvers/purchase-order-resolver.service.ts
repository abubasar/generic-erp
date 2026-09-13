import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { PurchaseOrderService } from "../services/purchase-order.service";

@Injectable({
  providedIn: "root",
})
export class PurchaseOrderResolverService  {
  constructor(private purchaseOrderService: PurchaseOrderService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.purchaseOrderService.getPurchaseOrderById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
