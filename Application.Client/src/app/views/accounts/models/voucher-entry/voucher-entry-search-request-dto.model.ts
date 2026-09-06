import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class VoucherEntrySearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  costCenterId?: string;
  paymentModeId?: string;
  voucherNo: string;
  voucherType?: number;
  voucherEntryStatus: number;
}
