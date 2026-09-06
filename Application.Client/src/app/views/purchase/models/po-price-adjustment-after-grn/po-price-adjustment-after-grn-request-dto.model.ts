export interface PoPriceAdjustmentAfterGrnRequestDTO {
  id?: string;
  adjustmentDate: string;
  referenceNo: string;
  grnno: string;
  ponumber: string;
  supplierId: string;
  storeId: string;
  totalAmount: number;
  remark: string;
  poPriceAdjustmentAfterGrnDetails: PoPriceAdjustmentAfterGrnRequestDetail[];
  deletedPoPriceAdjustmentAfterGrnDetailIds?: string;
}

interface PoPriceAdjustmentAfterGrnRequestDetail {
  id?: string;
  productId: string;
  grnQuantity?: number;
  grnRate?: number;
  adjustmentQuantity: number;
  adjustmentRate: number;
  amount: number;
}
