export interface VendorQuotationRequestDTO {
  id?: string;
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
  totalAmount: number;
  deletedVendorQuotationDetailIds?: string;
  vendorQuotationDetails: VendorQuotationRequestDetail[];
}

interface VendorQuotationRequestDetail {
  id?: string;
  productId: string;
  quantity: number;
  rate: number;
  amount: number;
}
