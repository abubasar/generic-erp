import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class SaleInvoiceSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  storeId: string;
  customerId: string;
  saleInvoiceStatus?: number;
  SaleInvoiceStatuses?: number[];
}
