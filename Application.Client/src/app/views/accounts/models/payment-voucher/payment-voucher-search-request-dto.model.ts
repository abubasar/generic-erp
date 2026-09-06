import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class PaymentVoucherSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  transactionNumber: string;
  costCenterId: string;
  paymentModeId: string;
  accountId: string;
  voucherNo: string;
  paymentVoucherStatus: number;
  accountTransactionType: number;
}
