import { Department } from "../department/department.model";

export interface Designation {
  id?: string;
  name: string;
  departmentId: string;
  department?: Department;
}
