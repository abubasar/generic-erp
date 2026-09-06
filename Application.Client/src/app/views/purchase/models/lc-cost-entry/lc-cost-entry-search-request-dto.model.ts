import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class LCCostEntrySearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  ponumber: string;
  lcNumber: string;
  costCenterId: string;
  lcCostEntryStatus: number;
}
