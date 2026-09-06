import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, Resolve } from "@angular/router";
import { SaleOrderService } from "../services/sale-order.service";
import { Observable, catchError, of } from "rxjs";

@Injectable({
  providedIn: "root",
})
export class SaleOrderResolverService implements Resolve<any> {
  constructor(private saleOrderService: SaleOrderService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    console.log("Called Get Sale Return in resolver...", route, id);
    return this.saleOrderService.getSaleOrderById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
