export interface PurchaseReturnRequestDTO {
  id?: string;
  purchaseReturnDate: string;
  referenceNo: string;
  grnno: string;
  supplierId: string;
  storeId: string;
  totalAmount: number;
  remark: string;
  purchaseReturnDetails: PurchaseReturnRequestDetail[];
  deletedPurchaseReturnDetailIds?: string;
}

interface PurchaseReturnRequestDetail {
  id?: string;
  productId: string;
  grnquantity?: number;
  grnBagWeightDeductionQty?: number;
  quantity: number;
  bagWeightDeductionQty: number;
  numberOfBagQuantity: number;
  netQuantity: number;
  rate: number;
  amount: number;
}
