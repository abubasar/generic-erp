import { Account } from "app/views/configuration/models/account/account.model";
import { ControlAccount } from "app/views/configuration/models/account/control-account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { PaymentMode } from "app/views/configuration/models/payment-mode/payment-mode.model";

export interface VoucherEntryResponseDTO {
  id: string;
  voucherNo: string;
  voucherDate: string;
  costCenterId: string;
  paymentModeId: string;
  referenceNo: string;
  voucherType: number;
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
  costCenter: CostCenter;
  paymentMode: PaymentMode;
  voucherEntryDetails: VoucherEntryResponseDetail[];
}

export interface VoucherEntryResponseDetail {
  id?: string;
  voucherEntryId?: string;
  accountId: string;
  accountDescription: string;
  amount: number;
  account?: Account;
}
