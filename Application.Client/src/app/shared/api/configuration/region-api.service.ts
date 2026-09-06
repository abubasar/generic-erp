import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { RegionRequest } from "app/views/configuration/models/region/region-request.model";
import { Region } from "app/views/configuration/models/region/region.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class RegionApiService {
  baseURL = environment.apiURL + "/region";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/search", { page: -1 });
  }
  getAll(request: RegionRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(region: Region) {
    return this.httpClient.post(this.baseURL, region);
  }

  update(region: Region) {
    return this.httpClient.post(this.baseURL + "/update", region);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
