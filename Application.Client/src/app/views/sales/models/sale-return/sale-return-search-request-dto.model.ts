import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class SaleReturnSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  storeId: string;
  customerId: string;
  saleReturnNo: string;
  saleReturnStatus: number;
}
