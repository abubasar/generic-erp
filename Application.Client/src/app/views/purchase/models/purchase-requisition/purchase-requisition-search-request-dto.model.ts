import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class PurchaseRequisitionSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  requisitionStatusIds?: number[];
}
