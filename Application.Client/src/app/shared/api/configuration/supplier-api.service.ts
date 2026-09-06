import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { SupplierRequest } from "app/views/configuration/models/supplier/supplier-request.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class SupplierApiService {
  baseURL = environment.apiURL + "/supplier";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/all", {});
  }

  getAll(request: SupplierRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(supplier: Supplier) {
    return this.httpClient.post(this.baseURL, supplier);
  }

  update(supplier: Supplier) {
    return this.httpClient.post(this.baseURL + "/update", supplier);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
