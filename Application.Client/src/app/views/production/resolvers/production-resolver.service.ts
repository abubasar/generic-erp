import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { ProductionService } from "../services/production.service";

@Injectable({
  providedIn: "root",
})
export class ProductionResolverService  {
  constructor(private productionService: ProductionService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.productionService.getProductionById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
