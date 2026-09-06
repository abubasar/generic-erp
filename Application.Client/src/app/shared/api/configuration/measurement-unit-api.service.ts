import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { MeasurementUnitRequest } from "app/views/configuration/models/measurement-unit/measurement-unit-request.model";
import { MeasurementUnit } from "app/views/configuration/models/measurement-unit/measurement-unit.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class MeasurementUnitApiService {
  baseURL = environment.apiURL + "/measurementUnit";

  constructor(private httpClient: HttpClient) {}

  getAll(request: MeasurementUnitRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(measurementUnit: MeasurementUnit) {
    return this.httpClient.post(this.baseURL, measurementUnit);
  }

  update(measurementUnit: MeasurementUnit) {
    return this.httpClient.post(this.baseURL + "/update", measurementUnit);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
