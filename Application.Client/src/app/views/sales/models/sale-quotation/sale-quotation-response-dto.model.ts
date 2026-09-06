import { Customer } from "app/views/configuration/models/customer/customer.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";

export interface SaleQuotationResponseDTO {
  id?: string;
  quotationNo?: string;
  quotationDate: string;
  customerId: string;
  referenceNo: string;
  expiryDate: string;
  termAndCondition: string;
  subtotal: number;
  discount: number;
  total: number;
  isMailSent: boolean;
  status?: number;
  statusName: string;
  remark: string;
  checkedBy: string;
  approvedBy: string;
  createdOn: string;
  updatedOn: string;
  createdBy: string;
  updatedBy: string;
  financialYearId: string;
  customer?: Customer;
  saleQuotationDetails: SaleQuotationResponseDetail[];
}

export interface SaleQuotationResponseDetail {
  id?: string;
  // saleQuotationId?: string;
  productId: string;
  measurementUnitName?: string;
  bagWeight: number;
  primaryQuantity: number;
  quantity: number;
  rate: number;
  discountAmount: number;
  discountPerUnit: number;
  amount: number;
  product: ProductView;
  createdOn?: string;
  createdBy?: string;
}
