import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class LcAdjustmentSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  purchaseInvoiceNo: string;
  costCenterId: string;
  lcAdjustmentStatus: number;
}
