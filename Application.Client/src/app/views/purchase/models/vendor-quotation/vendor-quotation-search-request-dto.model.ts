import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class VendorQuotationSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  requisitionNo: string;
  vendorQuotationStatus: number;
}
