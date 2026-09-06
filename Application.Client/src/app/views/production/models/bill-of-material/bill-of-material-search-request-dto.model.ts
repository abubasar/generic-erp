import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class BillOfMaterialSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  finishedProductId: string;
  formulationNo: string;
  bomStatus?: number;
}
