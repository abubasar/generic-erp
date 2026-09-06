import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";

export interface VendorQuotationResponseDTO {
  id?: string;
  quotationNo?: string;
  requisitionNo: string;
  referenceNo: string;
  storeId: string;
  supplierId: string;
  transport: number;
  paymentMode: number;
  paymentTermInDays: number;
  deliveryTermInDays: number;
  deliveryDate: string;
  currencyId: string;
  importPurchaseIncoTerm: number;
  importPurchasePaymentTerm: number;
  termAndCondition: string;
  remark: string;
  status: number;
  totalAmount: number;
  approvedBy: string;
  createdOn: string;
  updatedOn: string;
  createdBy: string;
  updatedBy: string;
  financialYearId: string;
  supplier?: Supplier;
  vendorQuotationDetails: VendorQuotationResponseDetail[];
}

export interface VendorQuotationResponseDetail {
  id?: string;
  // vendorQuotationId?: string;
  productId: string;
  measurementUnitName?: string;
  quantity: number;
  rate: number;
  amount: number;
  product?: ProductView;
}
