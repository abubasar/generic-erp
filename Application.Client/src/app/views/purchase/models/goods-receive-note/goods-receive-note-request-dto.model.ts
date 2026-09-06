export interface GoodsReceiveNoteRequestDTO {
  id?: string;
  grndate: string;
  ponumber?: string;
  storeId: string;
  supplierId: string;
  challanNo: string;
  challanDate: string;
  truckNo: string;
  driverName: string;
  driverContactNo: string;
  transport?: number;
  paymentTermInDays: number;
  isImportPurchase: boolean;
  proformaInvoiceNo: string;
  lcNumber: string;
  exchangeRate: number;
  currencyId: string;
  importPurchaseIncoTerm: number;
  importPurchasePaymentTerm: number;
  portOfLoading: string;
  portOfDestination: string;
  status: number;
  subtotal: number;
  discount: number;
  transportationCost: number;
  total: number;
  totalGrnAdjustmentAmount: number;
  purchaseOrderTotal: number;
  remark: string;
  goodsReceiveNoteDetails?: GoodsReceiveNoteDetail[];
}

interface GoodsReceiveNoteDetail {
  id?: string;
  productId: string;
  poquantity: number;
  grnquantity: number;
  bagWeightDeductionQuantity: number;
  numberOfBagQuantity: number;
  netQuantity: number;
  batchNo?: number;
  expiryDate?: string;
  rejectedQuantity: number;
  rejectionReason: string;
  currencyRate: number;
  rate: number;
  rateAfterBagWeightDeduction: number;
  currencyAmount: number;
  amount: number;
}
