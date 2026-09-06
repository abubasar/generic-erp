import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { LcAdjustmentService } from "../services/lc-adjustment.service";

@Injectable({
  providedIn: "root",
})
export class LcAdjustmentResolverService {
  constructor(private lcAdjustmentService: LcAdjustmentService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.lcAdjustmentService.getLcAdjustmentById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
