import { ProductType } from "../product-type/product-type.model";

export interface Generic {
  id: string;
  name: string;
  productTypeId: string;
  productType: ProductType;
}
