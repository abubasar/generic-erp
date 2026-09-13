import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { PurchaseRequisitionService } from "../services/purchase-requisition.service";

@Injectable({
  providedIn: "root",
})
export class PurchaseRequisitionResolverService  {
  constructor(private purchaseRequisitionService: PurchaseRequisitionService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.purchaseRequisitionService.getPurchaseRequisitionById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
