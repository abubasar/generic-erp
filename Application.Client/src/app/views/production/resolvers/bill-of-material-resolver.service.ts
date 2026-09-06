import { Injectable } from "@angular/core";
import { Resolve, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { BillOfMaterialService } from "../services/bill-of-material.service";

@Injectable({
  providedIn: "root",
})
export class BillOfMaterialResolverService implements Resolve<any> {
  constructor(private billOfMaterialService: BillOfMaterialService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.billOfMaterialService.getBillOfMaterialById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
