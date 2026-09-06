export interface SaleReturnRequestDTO {
  id?: string;
  deliveryNoteNo?: string;
  invoiceNo?: string;
  saleReturnDate: string;
  referenceNo: string;
  storeId: string;
  customerId: string;
  customerMarketingOfficerId: string;
  customerTerritoryId: string;
  subtotal: number;
  totalPercentageDiscountAmount: number;
  otherDiscount: number;
  total: number;
  remark: string;
  saleReturnDetails: SaleReturnRequestDetail[];
  deletedSaleReturnDetailIds?: string;
}

interface SaleReturnRequestDetail {
  id?: string;
  productId: string;
  bagWeight: string;
  primaryQuantity: number;
  quantity: number;
  primaryBonusQuantity: number;
  bonusQuantity: number;
  rate: number;
  returnPrimaryQuantity: number;
  returnQuantity: number;
  returnPrimaryBonusQuantity: number;
  returnBonusQuantity: number;
  vatPercentage: number;
  discountPercentage: number;
  percentageDiscountAmount: number;
  otherDiscountPerUnit: number;
  amount: number;
}
