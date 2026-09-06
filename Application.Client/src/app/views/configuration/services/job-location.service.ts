import { Injectable } from "@angular/core";
import { JobLocationApiService } from "app/shared/api/configuration/job-location-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { Observable, map } from "rxjs";
import { JobLocationRequest } from "../models/job-location/job-location-request.model";
import { JobLocation } from "../models/job-location/job-location.model";

@Injectable({
  providedIn: "root",
})
export class JobLocationService {
  constructor(private api: JobLocationApiService) {}

  getJobLocations(
    jobLocationRequest: JobLocationRequest
  ): Observable<SearchResponse<JobLocation>> {
    return this.api
      .getAll(jobLocationRequest)
      .pipe(map((response: SearchResponse<JobLocation>) => response));
  }

  createJobLocation(
    jobLocation: JobLocation
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(jobLocation)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateJobLocation(
    jobLocation: JobLocation
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(jobLocation)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteJobLocation(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
