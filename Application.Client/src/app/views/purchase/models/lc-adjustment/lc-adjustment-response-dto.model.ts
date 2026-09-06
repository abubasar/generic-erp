import { Account } from "app/views/configuration/models/account/account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { PurchaseInvoiceResponseDTO } from "../purchase-invoice/purchase-invoice-response-dto.model";

export interface LcAdjustmentResponseDTO {
  id: string;
  code: string;
  purchaseInvoiceId: string;
  purchaseInvoiceNo: string;
  adjustmentDate: string;
  costCenterId: string;
  invoiceTotal: number;
  lcMarginTotal: number;
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
  purchaseInvoice: PurchaseInvoiceResponseDTO;
  lcAdjustmentDetails: LcAdjustmentResponseDetail[];
}

export interface LcAdjustmentResponseDetail {
  id?: string;
  lcAdjustmentId?: string;
  accountId: string;
  accountDescription: string;
  postType: number;
  amount: number;
  account?: Account;
}
