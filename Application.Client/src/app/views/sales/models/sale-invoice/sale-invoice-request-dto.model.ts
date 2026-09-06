export interface SaleInvoiceRequestDTO {
  id?: string;
  invoiceDate: string;
  deliveryNoteNo: string;
  saleOrderNo: string;
  orderDate: string;
  storeId?: string;
  transport: number;
  customerId: string;
  customerMarketingOfficerId: string;
  customerTerritoryId: string;
  subtotal: number;
  totalPercentageDiscountAmount: number;
  discount: number;
  offerDiscount: number;
  otherDiscount: number;
  total: number;
  transportationCost: number;
  depoCharge: number;
  netTotal: number;
  referenceNo: string;
  bankDetails: string;
  termAndCondition: string;
  remark: string;
  paymentTerm: number;
  saleInvoiceDetails?: SaleInvoiceDetail[];
}

interface SaleInvoiceDetail {
  id?: string;
  productId: string;
  primaryQuantity: number;
  primaryBonusQuantity: number;
  quantity: number;
  bonusQuantity: number;
  rate: number;
  vatPercentage: number;
  discountPercentage: number;
  percentageDiscountAmount: number;
  netRate: number;
  discountPerUnit: number;
  offerDiscountPerUnit: number;
  discountAmount: number;
  invoiceDiscountPerUnit: number;
  cashDiscountPerUnit: number;
  specialDiscountPerUnit: number;
  amount: number;
  deliveryNoteNo: string;
  deliveryDate: string;
  deliveryPlace: string;
  otherDiscountPerUnit: number;
  transportationCostPerUnit: number;
  depoChargePerUnit: number;
}
