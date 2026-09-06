import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class CustomerWiseProductDiscountRequest extends BaseRequest {
  fromDate: string;
  toDate: string;
  customerId: string;
  customerZoneId: string;
  customerAreaId: string;
  customerMarketingOfficerId: string;
  isActive: boolean;
  customerWiseProductDiscountStatus: number;
}
