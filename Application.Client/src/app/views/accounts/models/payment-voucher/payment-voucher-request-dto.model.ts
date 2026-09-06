import { ControlAccount } from "app/views/configuration/models/account/control-account.model";

export interface PaymentVoucherRequestDTO {
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
  paymentVoucherDetails: PaymentVoucherRequestDetail[];
  deletedPaymentVoucherDetailIds?: string;
}

export interface PaymentVoucherRequestDetail {
  id?: string;
  accountId: string;
  accountDescription: string;
  amount: number;
  account?: ControlAccount;
}
