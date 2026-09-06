import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class PurchaseInvoiceSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  grnno: string;
  supplierId: string;
  isImportPurchase: boolean;
  purchaseInvoiceStatus?: number;
  purchaseInvoiceStatuses?: number[];
}
