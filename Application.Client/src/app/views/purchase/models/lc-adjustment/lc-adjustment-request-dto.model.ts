export interface LcAdjustmentRequestDTO {
  id?: string;
  purchaseInvoiceId: string;
  purchaseInvoiceNo: string;
  adjustmentDate: string;
  costCenterId?: string;
  invoiceTotal: number;
  lcMarginTotal: number;
  remark: string;
  lcAdjustmentDetails: LcAdjustmentRequestDetail[];
  deletedLcAdjustmentDetailIds?: string;
}

interface LcAdjustmentRequestDetail {
  id?: string;
  accountId: string;
  accountDescription: string;
  postType: number;
  amount: number;
}
