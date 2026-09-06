export interface CustomerWiseProductDiscountByCustomerId {
  productId: string;
  customerWiseProductDiscountId: string;
  invoiceDiscountPerUnit: number;
  cashDiscountPerUnit: number;
  specialDiscountPerUnit: number;
}
