import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class PurchaseReturnSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  supplierId: string;
  purchaseReturnNo: string;
  purchaseReturnStatus: number;
}
