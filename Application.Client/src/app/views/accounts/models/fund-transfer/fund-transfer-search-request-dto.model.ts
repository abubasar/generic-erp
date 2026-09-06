import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class FundTransferSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  costCenterId: string;
  transferFromAccountId: string;
  transferToAccountId: string;
  fundTransferStatus: number;
}
