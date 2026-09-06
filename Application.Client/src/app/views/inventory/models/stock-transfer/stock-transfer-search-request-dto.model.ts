import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class StockTransferSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  stockTransferStatus: number;
}
