import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class DiscountProductWiseRequest extends BaseRequest {
  startDate: string;
  endDate: string;
}
