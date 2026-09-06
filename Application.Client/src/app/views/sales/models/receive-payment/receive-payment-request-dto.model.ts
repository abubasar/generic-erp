import { Account } from "app/views/configuration/models/account/account.model";

export interface ReceivePaymentRequestDTO {
  id?: string;
  paymentDate: string;
  customerId: string;
  fundTransferTransactionTypeId: string;
  transactionNumber: string;
  paymentModeId: string;
  totalAmount: number;
  feedSalesPurpose: number;
  creditRecoveryPurpose: number;
  costCenterId: string;
  remark: string;
  receivePaymentDetails: ReceivePaymentRequestDetail[];
  deletedReceivePaymentDetailIds?: string;
}

export interface ReceivePaymentRequestDetail {
  id?: string;
  accountId: string;
  accountDescription: string;
  amount: number;
  account?: Account;
}
