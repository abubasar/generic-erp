import { Injectable } from "@angular/core";
import { DepartmentApiService } from "app/shared/api/configuration/department-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { DepartmentRequest } from "../models/department/department-request.model";
import { Department } from "../models/department/department.model";

@Injectable({
  providedIn: "root",
})
export class DepartmentService {
  constructor(private api: DepartmentApiService) {}

  getAllDepartments(): Observable<SearchResponse<Department>> {
    return this.api
      .getAlls()
      .pipe(map((response: SearchResponse<Department>) => response));
  }

  getDepartments(
    departmentRequest: DepartmentRequest
  ): Observable<SearchResponse<Department>> {
    return this.api
      .getAll(departmentRequest)
      .pipe(map((response: SearchResponse<Department>) => response));
  }

  createDepartment(
    department: Department
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(department)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateDepartment(
    department: Department
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(department)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteDepartment(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
