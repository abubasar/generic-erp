import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class SaleOrderSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  storeId: string;
  customerId: string;
  saleOrderNo: string;
  quotationNo: string;
  saleOrderStatuses: number[];
  saleOrderStatus: number;
}
