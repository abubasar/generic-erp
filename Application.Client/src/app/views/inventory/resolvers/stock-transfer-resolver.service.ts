import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, Resolve } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { StockTransferService } from "../services/stock-transfer.service";

@Injectable({
  providedIn: "root",
})
export class StockTransferResolverService implements Resolve<any> {
  constructor(private stockTransferService: StockTransferService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.stockTransferService.getStockTransferById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
