import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { TerritoryRequest } from "app/views/configuration/models/Territory/territory-request.model";
import { Territory } from "app/views/configuration/models/Territory/territory.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class TerritoryApiService {
  baseURL = environment.apiURL + "/territory";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/search", { page: -1 });
  }

  getAll(request: TerritoryRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(territory: Territory) {
    return this.httpClient.post(this.baseURL, territory);
  }

  update(territory: Territory) {
    return this.httpClient.post(this.baseURL + "/update", territory);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
