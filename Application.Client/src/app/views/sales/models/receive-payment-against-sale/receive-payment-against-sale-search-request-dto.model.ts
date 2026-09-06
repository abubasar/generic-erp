import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class ReceivePaymentAgainstSaleSearchRequestDTO extends BaseRequest {
  customerId: string;
  costCenterId: string;
  receivePaymentStatus: number;
}
