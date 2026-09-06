import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { PackSizeRequest } from "app/views/configuration/models/pack-size/pack-size-request.model";
import { PackSize } from "app/views/configuration/models/pack-size/pack-size.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class PackSizeApiService {
  baseURL = environment.apiURL + "/packSize";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/all", {});
  }

  getAll(request: PackSizeRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(packSize: PackSize) {
    return this.httpClient.post(this.baseURL, packSize);
  }

  update(packSize: PackSize) {
    return this.httpClient.post(this.baseURL + "/update", packSize);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
