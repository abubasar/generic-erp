import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class SaleQuotationSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  quotationNo: string;
  saleQuotationStatus?: number;
}
