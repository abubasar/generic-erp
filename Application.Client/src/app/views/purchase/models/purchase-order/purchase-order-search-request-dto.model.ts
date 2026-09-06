import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class PurchaseOrderSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  supplierId: string;
  ponumber: string;
  isImportPurchase: boolean;
  purchaseOrderStatuses: number[];
  purchaseOrderStatus?: number;
}
