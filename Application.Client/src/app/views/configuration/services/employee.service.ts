import { Injectable } from "@angular/core";
import { EmployeeApiService } from "app/shared/api/configuration/employee-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { EmployeeRequest } from "../models/employee/employee-request.model";
import { Employee } from "../models/employee/employee.model";

@Injectable({
  providedIn: "root",
})
export class EmployeeService {
  constructor(private api: EmployeeApiService) {}

  getAllEmployees(): Observable<SearchResponse<Employee>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Employee>) => response));
  }
  getEmployees(
    employeeRequest: EmployeeRequest
  ): Observable<SearchResponse<Employee>> {
    return this.api
      .getAll(employeeRequest)
      .pipe(map((response: SearchResponse<Employee>) => response));
  }

  createEmployee(employee: Employee): Observable<GeneralResponse<string>> {
    return this.api
      .create(employee)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateEmployee(employee: Employee): Observable<GeneralResponse<string>> {
    return this.api
      .update(employee)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteEmployee(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
