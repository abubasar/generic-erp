import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class SupplierPaymentSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  transactionNumber: string;
  paymentModeId: string;
  supplierId: string;
  costCenterId: string;
  supplierPaymentStatuses: number[];
  supplierPaymentStatus: number;
  supplierPaymentType: number;
}
