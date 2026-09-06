import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class ProductTypeRequest extends BaseRequest {
  inventoryTypeId: string;
}
