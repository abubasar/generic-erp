import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { VoucherEntryService } from "../services/voucher-entry.service";

@Injectable({
  providedIn: "root",
})
export class VoucherEntryResolverService  {
  constructor(private voucherEntryService: VoucherEntryService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.voucherEntryService.getVoucherEntryById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
