import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class ProductionSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  productionStatus?: number;
  finishedProductId?: string;
}
