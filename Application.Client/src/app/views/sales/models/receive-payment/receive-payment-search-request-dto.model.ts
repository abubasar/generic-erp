import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class ReceivePaymentSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  transactionNumber: string;
  paymentModeId: string;
  customerId: string;
  costCenterId: string;
  receivePaymentStatus: number;
  receivePaymentStatuses: number[];
}
