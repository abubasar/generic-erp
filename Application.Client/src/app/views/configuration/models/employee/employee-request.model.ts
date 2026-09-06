import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class EmployeeRequest extends BaseRequest {
  departmentId: string;
  designationId: string;
  jobLocationId: string;
  employeeIdNo: string;
}
