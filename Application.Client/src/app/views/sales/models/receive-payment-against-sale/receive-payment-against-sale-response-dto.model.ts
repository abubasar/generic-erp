import { Account } from "app/views/configuration/models/account/account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { Customer } from "app/views/configuration/models/customer/customer.model";

export interface ReceivePaymentAgainstSaleResponseDTO {
  id?: string;
  code: string;
  paymentDate: string;
  invoiceNo: string;
  customerId: string;
  amount: number;
  costCenterId: string;
  toAccountId: string;
  remark: string;
  status: number;
  statusName: string;
  checkedBy: string;
  approvedBy: string;
  createdOn: string;
  updatedOn: string;
  createdBy: string;
  updatedBy: string;
  costCenter: CostCenter;
  toAccount: Account;
  customer: Customer;
}
