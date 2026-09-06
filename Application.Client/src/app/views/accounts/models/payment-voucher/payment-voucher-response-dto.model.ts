import { ControlAccount } from "app/views/configuration/models/account/control-account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { FundTransferTransactionType } from "app/views/configuration/models/fund-transfer-transaction-type/fund-transfer-transaction-type.model";
import { PaymentMode } from "app/views/configuration/models/payment-mode/payment-mode.model";

export interface PaymentVoucherResponseDTO {
  id: string;
  voucherNo: string;
  voucherDate: string;
  fundTransferTransactionTypeId: string;
  transactionNumber: string;
  costCenterId: string;
  paymentModeId: string;
  referenceNo: string;
  cashBankAccountId: string;
  totalAmount: number;
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
  cashBankAccount?: ControlAccount;
  fundTransferTransactionType: FundTransferTransactionType;
  costCenter: CostCenter;
  paymentMode: PaymentMode;
  paymentVoucherDetails: PaymentVoucherResponseDetail[];
}

export interface PaymentVoucherResponseDetail {
  id?: string;
  paymentVoucherId?: string;
  accountId: string;
  accountDescription: string;
  amount: number;
  account?: ControlAccount;
}
