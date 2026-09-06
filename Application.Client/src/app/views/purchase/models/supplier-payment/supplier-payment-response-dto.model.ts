import { Account } from "app/views/configuration/models/account/account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { FundTransferTransactionType } from "app/views/configuration/models/fund-transfer-transaction-type/fund-transfer-transaction-type.model";
import { PaymentMode } from "app/views/configuration/models/payment-mode/payment-mode.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";

export interface SupplierPaymentResponseDTO {
  id: string;
  code: string;
  supplierPaymentType: number;
  SupplierPaymentTypeName: string;
  ponumber: string;
  paymentDate: string;
  fundTransferTransactionTypeId: string;
  transactionNumber: string;
  supplierId: string;
  paymentModeId: string;
  totalAmount: number;
  usedAmountInPurchaseInvoice: number;
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
  fundTransferTransactionType: FundTransferTransactionType;
  supplier: Supplier;
  paymentMode: PaymentMode;
  supplierPaymentDetails: SupplierPaymentResponseDetail[];
}

export interface SupplierPaymentResponseDetail {
  id?: string;
  supplierPaymentId?: string;
  accountId: string;
  accountDescription: string;
  purchaseInvoiceNo?: string;
  amount: number;
  account?: Account;
}
