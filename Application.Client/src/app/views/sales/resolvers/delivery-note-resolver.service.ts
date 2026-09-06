import { Injectable } from "@angular/core";
import { Resolve, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { DeliveryNoteService } from "../services/delivery-note.service";

@Injectable({
  providedIn: "root",
})
export class DeliveryNoteResolverService implements Resolve<any> {
  constructor(private deliveryNoteService: DeliveryNoteService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.deliveryNoteService.getDeliveryNoteById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
