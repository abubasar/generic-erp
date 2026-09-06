import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, Resolve } from "@angular/router";
import { Observable, of } from "rxjs";
import { catchError } from "rxjs/operators";
import { SaleReturnResponseDTO } from "../models/sale-return/sale-return-response-dto.model";
import { SaleReturnService } from "../services/sale-return.service";

@Injectable({
  providedIn: "root",
})
export class SaleReturnResolverService implements Resolve<any> {
  constructor(private saleReturnService: SaleReturnService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    console.log("Called Get Sale Return in resolver...", route, id);
    return this.saleReturnService.getSaleReturnById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
