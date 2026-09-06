import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class GoodsReceiveNoteSearchRequestDTO extends BaseRequest {
  fromDate: string;
  toDate: string;
  supplierId: string;
  ponumber: string;
  grnno: string;
  isImportPurchase: boolean;
  grnStatus?: number;
  grnStatuses?: number[];
}
