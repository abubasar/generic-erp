import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { InventoryTypeRequest } from "app/views/configuration/models/inventory-type/inventory-type-request.model";
import { InventoryType } from "app/views/configuration/models/inventory-type/inventory-type.model";

import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class InventoryTypeApiService {
  baseURL = environment.apiURL + "/inventoryType";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/all", {});
  }
  getAll(request: InventoryTypeRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(inventoryType: InventoryType) {
    return this.httpClient.post(this.baseURL, inventoryType);
  }

  update(inventoryType: InventoryType) {
    return this.httpClient.post(this.baseURL + "/update", inventoryType);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
