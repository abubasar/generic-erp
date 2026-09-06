import { Injectable } from "@angular/core";
import { MeasurementUnitApiService } from "app/shared/api/configuration/measurement-unit-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { MeasurementUnitRequest } from "../models/measurement-unit/measurement-unit-request.model";
import { MeasurementUnit } from "../models/measurement-unit/measurement-unit.model";

@Injectable({
  providedIn: "root",
})
export class MeasurementUnitService {
  constructor(private api: MeasurementUnitApiService) {}

  getMeasurementUnits(
    departmentRequest: MeasurementUnitRequest
  ): Observable<SearchResponse<MeasurementUnit>> {
    return this.api
      .getAll(departmentRequest)
      .pipe(map((response: SearchResponse<MeasurementUnit>) => response));
  }

  createMeasurementUnit(
    measurementUnit: MeasurementUnit
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(measurementUnit)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateMeasurementUnit(
    measurementUnit: MeasurementUnit
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(measurementUnit)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteMeasurementUnit(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
