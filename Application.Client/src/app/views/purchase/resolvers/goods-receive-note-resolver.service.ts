import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot } from "@angular/router";
import { Observable, catchError, of } from "rxjs";
import { GoodsReceiveNoteService } from "../services/goods-receive-note.service";

@Injectable({
  providedIn: "root",
})
export class GoodsReceiveNoteResolverService  {
  constructor(private goodsReceiveNoteService: GoodsReceiveNoteService) {}
  resolve(route: ActivatedRouteSnapshot): Observable<any> {
    const id = route.paramMap.get("id");
    return this.goodsReceiveNoteService.getGoodsReceiveNoteById(id).pipe(
      catchError((error) => {
        return of("No data");
      })
    );
  }
}
