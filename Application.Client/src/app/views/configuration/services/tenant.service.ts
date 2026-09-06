import { Injectable } from "@angular/core";
import { TenantApiService } from "app/shared/api/configuration/tenant-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { TenantRequest } from "../models/tenant/tenant-request.model";
import { Tenant } from "../models/tenant/tenant.model";

@Injectable({
  providedIn: "root",
})
export class TenantService {
  constructor(private api: TenantApiService) {}

  getTenants(tenantRequest: TenantRequest): Observable<SearchResponse<Tenant>> {
    return this.api
      .getAll(tenantRequest)
      .pipe(map((response: SearchResponse<Tenant>) => response));
  }

  getTenantById(id: string): Observable<GeneralResponse<Tenant>> {
    return this.api
      .getById(id)
      .pipe(map((response: GeneralResponse<Tenant>) => response));
  }

  createTenant(tenant: Tenant): Observable<GeneralResponse<string>> {
    return this.api
      .create(tenant)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  // updateTenant(tenant: Tenant): Observable<GeneralResponse<string>> {
  //   return this.api
  //     .update(tenant)
  //     .pipe(map((response: GeneralResponse<string>) => response));
  // }

  // deleteTenant(id: string): Observable<GeneralResponse<string>> {
  //   return this.api
  //     .delete(id)
  //     .pipe(map((response: GeneralResponse<string>) => response));
  // }
}
