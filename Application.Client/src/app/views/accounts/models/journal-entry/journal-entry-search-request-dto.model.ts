import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class JournalEntrySearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  voucherNo: string;
  costCenterId: string;
  journalEntryStatus: number;
}
