import { Injectable } from "@angular/core";
import { Resolve, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { PurchaseRequisitionService } from "../services/purchase-requisition.service";

@Injectable({
  providedIn: "root",
})
export class PurchaseRequisitionResolverService implements Resolve<any> {
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
