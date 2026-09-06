import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class DeliveryNoteSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  customerId: string;
  storeId: string;
  deliveryNoteNo: string;
  saleOrderNo: string;
  deliveryNoteStatus: number;
  deliveryNoteStatuses?:number[];
}
