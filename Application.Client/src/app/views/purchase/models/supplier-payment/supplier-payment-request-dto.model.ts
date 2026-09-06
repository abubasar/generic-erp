import { Account } from "app/views/configuration/models/account/account.model";

export interface SupplierPaymentRequestDTO {
  id?: string;
  supplierPaymentType: number;
  ponumber: string;
  fundTransferTransactionTypeId: string;
  transactionNumber: string;
  paymentDate: string;
  supplierId: string;
  paymentModeId: string;
  totalAmount: number;
  costCenterId: string;
  remark: string;
  supplierPaymentDetails: SupplierPaymentRequestDetail[];
  deletedSupplierPaymentDetailIds?: string;
}

export interface SupplierPaymentRequestDetail {
  id?: string;
  accountId: string;
  accountDescription: string;
  purchaseInvoiceNo?: string;
  amount: number;
  account?: Account;
}
