import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, Resolve } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { LcCostEntryService } from "../services/lc-cost-entry.service";

@Injectable({
  providedIn: "root",
})
export class LcCostEntryResolverService implements Resolve<any> {
  constructor(private lcCostEntryService: LcCostEntryService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.lcCostEntryService.getLCCostEntryById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
