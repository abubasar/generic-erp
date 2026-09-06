import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { TenantRequest } from "app/views/configuration/models/tenant/tenant-request.model";
import { Tenant } from "app/views/configuration/models/tenant/tenant.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class TenantApiService {
  baseURL = environment.apiURL + "/tenant";

  constructor(private httpClient: HttpClient) {}

  getAll(request: TenantRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  getById(tenantId: string) {
    return this.httpClient.get(this.baseURL + `${tenantId}`);
  }

  create(tenant: Tenant) {
    return this.httpClient.post(this.baseURL, tenant);
  }

  // update(tenant: Tenant) {
  //   return this.httpClient.post(this.baseURL + "/update", tenant);
  // }

  // delete(id: string) {
  //   return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  // }
}
