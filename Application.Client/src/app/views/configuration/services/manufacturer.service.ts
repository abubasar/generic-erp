import { Injectable } from "@angular/core";
import { ManufacturerApiService } from "app/shared/api/configuration/manufacturer-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { ManufacturerRequest } from "../models/manufacturer/manufacturer-request.model";
import { Manufacturer } from "../models/manufacturer/manufacturer.model";

@Injectable({
  providedIn: "root",
})
export class ManufacturerService {
  constructor(private api: ManufacturerApiService) {}

  getAllManufacturers(): Observable<SearchResponse<Manufacturer>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Manufacturer>) => response));
  }

  getManufacturers(
    manufacturerRequest: ManufacturerRequest
  ): Observable<SearchResponse<Manufacturer>> {
    return this.api
      .getAll(manufacturerRequest)
      .pipe(map((response: SearchResponse<Manufacturer>) => response));
  }

  createManufacturer(
    manufacturer: Manufacturer
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(manufacturer)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateManufacturer(
    manufacturer: Manufacturer
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(manufacturer)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteManufacturer(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
