import { Employee } from "../employee/employee.model";
import { Territory } from "../Territory/territory.model";

export interface Customer {
  id?: string;
  code?: string;
  name: string;
  ownersName: string;
  contactNo: string;
  email: string;
  address: string;
  nationalId: string;
  tradeLicense: string;
  contactPersonName: string;
  contactPersonContactNo: string;
  contactPersonEmail: string;
  contactPersonDesignation: string;
  customerRegionId: string;
  customerZoneId: string;
  customerAreaId: string;
  customerTerritoryId: string;
  customerMarketingOfficerId: string;
  customerCreditLimit: number;
  customerAgreement: boolean;
  customerCreditDays: string;
  customerTargetQuantity: string;
  bankAccounts: BankAccount[];
  customerMarketingOfficer: Employee;
  deletedBankAccountIds: string;
}

export interface BankAccount {
  id?: string;
  name: string;
  accNo: string;
  routingNo: string;
  bankName: string;
  branchName: string;
}
