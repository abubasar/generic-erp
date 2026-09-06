import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class ManufacturingOrderSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  manufacturingOrderStatus?: number;
}
