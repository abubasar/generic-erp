import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { JobLocationRequest } from 'app/views/configuration/models/job-location/job-location-request.model';
import { JobLocation } from 'app/views/configuration/models/job-location/job-location.model';
import { environment } from 'environments/environment';

@Injectable({
  providedIn: "root",
})
export class JobLocationApiService {
  baseURL = environment.apiURL + "/jobLocation";

  constructor(private httpClient: HttpClient) {}

  getAll(request: JobLocationRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(jobLocation: JobLocation) {
    return this.httpClient.post(this.baseURL, jobLocation);
  }

  update(jobLocation: JobLocation) {
    return this.httpClient.post(this.baseURL + "/update", jobLocation);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
