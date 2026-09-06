import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { ZoneRequest } from "app/views/configuration/models/zone/zone-request.model";
import { Zone } from "app/views/configuration/models/zone/zone.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class ZoneApiService {
  baseURL = environment.apiURL + "/zone";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/search", { page: -1 });
  }
  getAll(request: ZoneRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }
  create(zone: Zone) {
    return this.httpClient.post(this.baseURL, zone);
  }
  update(zone: Zone) {
    return this.httpClient.post(this.baseURL + "/update", zone);
  }
  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
