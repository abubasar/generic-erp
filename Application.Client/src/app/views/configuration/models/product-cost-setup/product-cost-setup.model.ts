import { ProductView } from "../product/product-view.model";

export interface ProductCostSetup {
  id?: string;
  productId: string;
  directExpense: number;
  factoryOverhead: number;
  product?: ProductView;
}
