import { Employee } from "../employee/employee.model";

export interface User {
  id?: string;
  username: string;
  roleId: string;
  roleName?: string;
  employeeId?: string;
  employee?:Employee;
  password?: string;
}
