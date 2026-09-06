import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { AreaRequest } from "app/views/configuration/models/area/area-request.model";
import { Area } from "app/views/configuration/models/area/area.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class AreaApiService {
  baseURL = environment.apiURL + "/area";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/search", { page: -1 });
  }
  getAll(request: AreaRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }
  create(area: Area) {
    return this.httpClient.post(this.baseURL, area);
  }
  update(area: Area) {
    return this.httpClient.post(this.baseURL + "/update", area);
  }
  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
