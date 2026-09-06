import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { GenericRequest } from "app/views/configuration/models/generic/generic-request.model";
import { Generic } from "app/views/configuration/models/generic/generic.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class GenericApiService {
  baseURL = environment.apiURL + "/generic";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/all", {});
  }

  getAll(request: GenericRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(generic: Generic) {
    return this.httpClient.post(this.baseURL, generic);
  }

  update(generic: Generic) {
    return this.httpClient.post(this.baseURL + "/update", generic);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
