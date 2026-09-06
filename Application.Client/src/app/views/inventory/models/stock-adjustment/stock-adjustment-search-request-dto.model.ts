import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class StockAdjustmentSearchRequestDTO extends BaseRequest {
    fromDate: string;
    toDate: string;
    code?: string;
    storeId?: string;
    stockAdjustmentStatus?: number;
  }