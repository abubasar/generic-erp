import { ControlAccount } from "app/views/configuration/models/account/control-account.model";

export interface ReceiveVoucherRequestDTO {
  id?: string;
  voucherDate: string;
  fundTransferTransactionTypeId: string;
  transactionNumber: string;
  costCenterId: string;
  paymentModeId: string;
  referenceNo: string;
  cashBankAccountId: string;
  totalAmount: number;
  remark: string;
  receiveVoucherDetails: ReceiveVoucherRequestDetail[];
  deletedReceiveVoucherDetailIds?: string;
}

export interface ReceiveVoucherRequestDetail {
  id?: string;
  accountId: string;
  accountDescription: string;
  amount: number;
  account?: ControlAccount;
}
