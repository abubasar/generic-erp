import { ProductView } from "app/views/configuration/models/product/product-view.model";

export interface SaleQuotationRequestDTO {
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
  remark: string;
  deletedSaleQuotationDetailIds?: string;
  saleQuotationDetails: SaleQuotationRequestDetail[];
}

export interface SaleQuotationRequestDetail {
  id?: string;
  productId: string;
  product?: ProductView;
  bagWeight: number;
  primaryQuantity: number;
  quantity: number;
  rate: number;
  discountAmount: number;
  discountPerUnit: number;
  amount: number;
  createdOn?: string;
  createdBy?: string;
}
