import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { RoleRequest } from "app/views/configuration/models/role/role-request.model";
import { Role } from "app/views/configuration/models/role/role.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class RoleApiService {
  baseURL = environment.apiURL + "/role";

  constructor(private httpClient: HttpClient) {}

  getAll(request: RoleRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(role: Role) {
    return this.httpClient.post(this.baseURL, role);
  }

  update(role: Role) {
    return this.httpClient.post(this.baseURL + "/update", role);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
