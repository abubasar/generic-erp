import { Injectable } from "@angular/core";
import { DesignationApiService } from "app/shared/api/configuration/designation-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { DesignationRequest } from "../models/designation/designation-request.model";
import { Designation } from "../models/designation/designation.model";

@Injectable({
  providedIn: "root",
})
export class DesignationService {
  constructor(private api: DesignationApiService) {}

  getDesignations(
    designationRequest: DesignationRequest
  ): Observable<SearchResponse<Designation>> {
    return this.api
      .getAll(designationRequest)
      .pipe(map((response: SearchResponse<Designation>) => response));
  }

  createDesignation(
    designation: Designation
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(designation)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateDesignation(
    designation: Designation
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(designation)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteDesignation(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
