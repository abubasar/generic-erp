import { ProductView } from "../product/product-view.model";

export interface DiscountProductWise {
  id?: string;
  name: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  discountProductWiseDetails: DiscountProductWiseDetail[];
  deletedDiscountProductWiseDetailIds?: string;
}

export interface DiscountProductWiseDetail {
  id?: string;
  discountProductWiseId?: string;
  productId: string;
  discountAmountPerKg: string;
  product?: ProductView;
}
