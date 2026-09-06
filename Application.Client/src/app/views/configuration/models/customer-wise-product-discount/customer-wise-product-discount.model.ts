import { Customer } from "../customer/customer.model";
import { ProductView } from "../product/product-view.model";

export interface CustomerWiseProductDiscount {
  id: string;
  customerId: string;
  applicableDate: string;
  isActive: boolean;
  status: number;
  statusName: string;
  checkedBy: string;
  approvedBy: string;
  customer?: Customer;
  createdOn?: string;
  createdBy?: string;
  customerWiseProductDiscountDetails: CustomerWiseProductDiscountDetail[];
  deletedCustomerWiseProductDiscountDetailIds?: string;
}

export interface CustomerWiseProductDiscountDetail {
  id?: string;
  customerWiseProductDiscountId?: string;
  productId: string;
  salePrice: number;
  invoiceDiscount: number;
  cashDiscount: number;
  specialDiscount: number;
  monthlyDiscount: number;
  yearlyDiscount: number;
  targetDiscount: number;
  product?: ProductView;
}
