import { Account } from "app/views/configuration/models/account/account.model";

export interface VoucherEntryRequestDTO {
  id?: string;
  voucherDate: string;
  costCenterId: string;
  paymentModeId: string;
  referenceNo: string;
  voucherType: number;
  cashBankAccountId: string;
  totalAmount: number;
  remark: string;
  voucherEntryDetails: VoucherEntryRequestDetail[];
  deletedVoucherEntryDetailIds?: string;
}

export interface VoucherEntryRequestDetail {
  id?: string;
  accountId: string;
  accountDescription: string;
  amount: number;
  account?: Account;
}
