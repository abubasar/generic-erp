import { Account } from "app/views/configuration/models/account/account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { Customer } from "app/views/configuration/models/customer/customer.model";
import { FundTransferTransactionType } from "app/views/configuration/models/fund-transfer-transaction-type/fund-transfer-transaction-type.model";
import { PaymentMode } from "app/views/configuration/models/payment-mode/payment-mode.model";
export interface ReceivePaymentResponseDTO {
  id: string;
  code: string;
  paymentDate: string;
  customerId: string;
  fundTransferTransactionTypeId: string;
  transactionNumber: string;
  paymentModeId: string;
  totalAmount: number;
  feedSalesPurpose: number;
  creditRecoveryPurpose: number;
  costCenterId: string;
  status: number;
  statusName: string;
  remark: string;
  checkedBy: string;
  approvedBy: string;
  createdOn: string;
  updatedOn: string;
  createdBy: string;
  updatedBy: string;
  financialYearId: string;
  costCenter: CostCenter;
  customer: Customer;
  fundTransferTransactionType: FundTransferTransactionType;
  paymentMode: PaymentMode;
  receivePaymentDetails: ReceivePaymentResponseDetail[];
}

export interface ReceivePaymentResponseDetail {
  id?: string;
  receivePaymentId?: string;
  accountId: string;
  accountDescription: string;
  amount: number;
  account?: Account;
}
