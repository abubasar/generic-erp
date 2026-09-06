import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, Resolve } from "@angular/router";
import { Observable, of } from "rxjs";
import { catchError } from "rxjs/operators";
import { DashboardDataService } from "../services/dashboard-data.service";

@Injectable({
  providedIn: "root",
})
export class DashboardResolver implements Resolve<any> {
  constructor(private dashboardDataService: DashboardDataService) {}

  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    return this.dashboardDataService.getDashboardStatistics(1).pipe(
      // map((data) => {
      //   return { data };
      // }),
      catchError((error) => {
        console.error(`Error fetching data: ${error}`);
        return of("No data");
      })
    );
  }
}
