import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class PoPriceAdjustmentAfterGrnSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  supplierId: string;
  code: string;
  poPriceAdjustmentAfterGrnStatus: number;
}
