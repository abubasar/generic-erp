import { Injectable } from "@angular/core";
import { PermissionsApiService } from "app/shared/api/configuration/permissions-api.service";
import { RoleApiService } from "app/shared/api/configuration/role-api.service";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { Permission, RoleClaim } from "../models/role/permission.model";
import { Role } from "../models/role/role.model";
import { RoleRequest } from "../models/role/role-request.model";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";

@Injectable({
  providedIn: "root",
})
export class RoleService {
  constructor(
    private api: RoleApiService,
    private permissionApi: PermissionsApiService
  ) {}

  getRoles(roleRequest: RoleRequest): Observable<SearchResponse<Role>> {
    return this.api
      .getAll(roleRequest)
      .pipe(map((response: SearchResponse<Role>) => response));
  }

  createRole(role: Role): Observable<GeneralResponse<string>> {
    return this.api
      .create(role)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateRole(role: Role): Observable<GeneralResponse<string>> {
    return this.api
      .update(role)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteRole(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  // Permissions Services
  getRolePermissionsByRoleId(
    roleId: string
  ): Observable<GeneralResponse<RoleClaim[]>> {
    return this.permissionApi
      .getPermissions(roleId)
      .pipe(map((response: GeneralResponse<RoleClaim[]>) => response));
  }

  updateRolePermissions(
    request: Permission
  ): Observable<GeneralResponse<string>> {
    return this.permissionApi
      .updateRolePermissions(request)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
