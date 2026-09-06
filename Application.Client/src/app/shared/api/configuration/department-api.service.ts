import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { DepartmentRequest } from "app/views/configuration/models/department/department-request.model";
import { Department } from "app/views/configuration/models/department/department.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class DepartmentApiService {
  baseURL = environment.apiURL + "/department";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/search", { page: -1 });
  }

  getAll(request: DepartmentRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(department: Department) {
    return this.httpClient.post(this.baseURL, department);
  }

  update(department: Department) {
    return this.httpClient.post(this.baseURL + "/update", department);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
