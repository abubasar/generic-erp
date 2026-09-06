import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Permission } from "app/views/configuration/models/role/permission.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class PermissionsApiService {
  baseURL = environment.apiURL + "/Auth/permissions";

  constructor(private httpClient: HttpClient) {}

  getPermissions(roleId: string) {
    return this.httpClient.get(this.baseURL + `/byrole/${roleId}`);
  }

  updateRolePermissions(request: Permission) {
    return this.httpClient.post(this.baseURL + "/update", request);
  }
}
