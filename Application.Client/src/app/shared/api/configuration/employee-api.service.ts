import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { EmployeeRequest } from "app/views/configuration/models/employee/employee-request.model";
import { Employee } from "app/views/configuration/models/employee/employee.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class EmployeeApiService {
  baseURL = environment.apiURL + "/employee";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/search", { page: -1 });
  }
  getAll(request: EmployeeRequest) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  create(employee: Employee) {
    return this.httpClient.post(this.baseURL, employee);
  }

  update(employee: Employee) {
    return this.httpClient.post(this.baseURL + "/update", employee);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }
}
