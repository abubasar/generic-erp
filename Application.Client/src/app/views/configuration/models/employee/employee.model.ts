import { Department } from "../department/department.model";
import { Designation } from "../designation/designation.model";
import { JobLocation } from "../job-location/job-location.model";

export interface Employee {
  id?: string;
  firstName: string;
  lastName: string;
  fullName?:string;
  employeeIdNo: string;
  contactNo: string;
  email: string;
  address: string;
  dateOfBirth: string;
  joiningDate: string;
  gender: number;
  genderName?: string;
  bloodGroup: number;
  bloodGroupName?: string;
  maritalStatus: number;
  maritalStatusName?: string;
  departmentId: string;
  designationId: string;
  jobLocationId: string;
  department?: Department;
  designation?: Designation;
  jobLocation?: JobLocation;
}
