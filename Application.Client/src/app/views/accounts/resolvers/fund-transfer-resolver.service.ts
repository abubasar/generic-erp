import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { FundTransferService } from "../services/fund-transfer.service";

@Injectable({
  providedIn: "root",
})
export class FundTransferResolverService {
  constructor(private fundTransferService: FundTransferService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.fundTransferService.getFundTransferById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
