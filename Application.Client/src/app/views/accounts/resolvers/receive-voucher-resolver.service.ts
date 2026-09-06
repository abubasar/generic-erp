import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, Resolve } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { ReceiveVoucherService } from "../services/receive-voucher.service";

@Injectable({
  providedIn: "root",
})
export class ReceiveVoucherResolverService implements Resolve<any> {
  constructor(private receiveVoucherService: ReceiveVoucherService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.receiveVoucherService.getReceiveVoucherById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
