import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { StoreRequest } from "app/views/configuration/models/store/store-request.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class StoreApiService {
  baseURL = environment.apiURL + "/store";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/all", {});
  }
  getAll(request: StoreRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(store: Store) {
    return this.httpClient.post(this.baseURL, store);
  }

  update(store: Store) {
    return this.httpClient.post(this.baseURL + "/update", store);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
