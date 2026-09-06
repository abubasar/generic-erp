import { Injectable } from "@angular/core";
import { Resolve, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { ManufacturingOrderService } from "../services/manufacturing-order.service";

@Injectable({
  providedIn: "root",
})
export class ManufacturingOrderResolverService implements Resolve<any> {
  constructor(private manufacturingOrderService: ManufacturingOrderService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.manufacturingOrderService.getManufacturingOrderById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
