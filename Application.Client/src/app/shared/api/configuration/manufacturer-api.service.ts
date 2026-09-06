import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ManufacturerRequest } from "app/views/configuration/models/manufacturer/manufacturer-request.model";
import { Manufacturer } from "app/views/configuration/models/manufacturer/manufacturer.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class ManufacturerApiService {
  baseURL = environment.apiURL + "/manufacturer";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/all", {});
  }
  getAll(request: ManufacturerRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(manufacturer: Manufacturer) {
    return this.httpClient.post(this.baseURL, manufacturer);
  }

  update(manufacturer: Manufacturer) {
    return this.httpClient.post(this.baseURL + "/update", manufacturer);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
