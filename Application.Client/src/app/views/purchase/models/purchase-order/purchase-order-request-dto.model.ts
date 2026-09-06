export interface PurchaseOrderRequestDTO {
  id?: string;
  requisitionNo: string;
  quotationNo?: string;
  referenceNo: string;
  podate: string;
  deliveryPlaceId: string;
  paymentMethodId: string;
  storeId: string;
  supplierId: string;
  productOrigin: string;
  packagingType: string;
  expiryTime: string;
  transport: number;
  paymentTermInDays: number;
  deliveryTermInDays: number;
  deliveryDate: string;
  paymentMode: number;
  weightVariance: number;
  isImportPurchase: boolean;
  proformaInvoiceNo: string;
  lcNumber: string;
  exchangeRate: number;
  currencyId: string;
  importPurchaseIncoTerm: number;
  importPurchasePaymentTerm: number;
  portOfLoading: string;
  portOfDestination: string;
  termAndCondition: string;
  subtotal: number;
  discount: number;
  total: number;
  remark: string;
  isPartialDelivery: boolean;
  purchaseOrderDetails?: PurchaseOrderRequestDetail[];
}

interface PurchaseOrderRequestDetail {
  id?: string;
  productId: string;
  quantity: number;
  receivedQuantity?: number;
  currencyRate: number;
  rate: number;
  currencyAmount: number;
  amount: number;
}
