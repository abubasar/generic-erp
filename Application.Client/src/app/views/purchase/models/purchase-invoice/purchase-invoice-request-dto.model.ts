export interface PurchaseInvoiceRequestDTO {
  id?: string;
  invoiceDate: string;
  grnno: string;
  ponumber: string;
  storeId: string;
  supplierId: string;
  supplierInvoiceNo: string;
  supplierInvoiceDate: string;
  paymentTermInDays: number;
  isImportPurchase: boolean;
  proformaInvoiceNo: string;
  lcNumber: string;
  currencyId: string;
  exchangeRate: number;
  importPurchaseIncoTerm: number;
  importPurchasePaymentTerm: number;
  billOfEntryNo: string;
  billOfEntryDate: string;
  portOfLoading: string;
  portOfDestination: string;
  additionalLandedCost: number;
  adjustmentValue: number;
  subtotal: number;
  discount: number;
  transportationCost: number;
  totalVat: number;
  supplierPaymentCode: string;
  advancePaymentAmount: number;
  totalGrnAdjustmentAmount: number;
  purchaseOrderTotal: number;
  netPayable: number;
  total: number;
  remark: string;
  purchaseInvoiceDetails?: PurchaseInvoiceDetail[];
}

interface PurchaseInvoiceDetail {
  id?: string;
  productId: string;
  grnquantity: number;
  bagWeightDeductionQuantity: number;
  numberOfBagQuantity: number;
  netQuantity: number;
  currencyRate: number;
  rate: number;
  rateAfterBagWeightDeduction: number;
  currencyAmount: number;
  amount: number;
  grnno?: string;
  grndate?: string;
  vatPercentage: number;
}
