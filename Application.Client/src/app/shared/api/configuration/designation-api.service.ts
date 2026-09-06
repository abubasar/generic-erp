import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";

import { DesignationRequest } from "app/views/configuration/models/designation/designation-request.model";
import { Designation } from "app/views/configuration/models/designation/designation.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class DesignationApiService {
  baseURL = environment.apiURL + "/designation";

  constructor(private httpClient: HttpClient) {}

  getAll(request: DesignationRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(designation: Designation) {
    return this.httpClient.post(this.baseURL, designation);
  }

  update(designation: Designation) {
    return this.httpClient.post(this.baseURL + "/update", designation);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
