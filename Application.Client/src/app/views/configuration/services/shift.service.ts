import { Injectable } from "@angular/core";
import { ShiftApiService } from "app/shared/api/configuration/shift-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { Observable, map } from "rxjs";
import { Shift } from "../models/shift/shift.model";
import { ShiftRequest } from "../models/shift/shift-request.model";

@Injectable({
  providedIn: "root",
})
export class ShiftService {
  constructor(private api: ShiftApiService) {}

  getAllShifts(): Observable<SearchResponse<Shift>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Shift>) => response));
  }

  getShifts(shiftRequest: ShiftRequest): Observable<SearchResponse<Shift>> {
    return this.api
      .getAll(shiftRequest)
      .pipe(map((response: SearchResponse<Shift>) => response));
  }

  createShift(shift: Shift): Observable<GeneralResponse<string>> {
    return this.api
      .create(shift)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateShift(shift: Shift): Observable<GeneralResponse<string>> {
    return this.api
      .update(shift)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteShift(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
