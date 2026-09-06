import { BaseRequest } from "app/shared/models/wrappers/baseRequest.model";

export class ProductRequest extends BaseRequest {
  productTypeId: string;
  inventoryTypeId: string;
  categoryId: string;
  genericId: string;
  packSizeId: string;
  countryId: string;
  isPurchaseProduct: boolean;
  isSaleProduct: boolean;
  inventoryTypeIds: string[];
}
