import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, Resolve } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { StockAdjustmentService } from "../services/stock-adjustment.service";

@Injectable({
  providedIn: "root",
})
export class StockAdjustmentResolverService implements Resolve<any> {
  constructor(private stockAdjustmentService: StockAdjustmentService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.stockAdjustmentService.getStockAdjustmentById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
