import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class CustomerRequest extends BaseRequest {
  customerRegionId: string;
  customerZoneId: string;
  customerAreaId: string;
  customerTerritoryId: string;
  customerMarketingOfficerId: string;
}
