import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";

export interface StockAdjustmentResponseDTO {
  id?: string;
  code?: string;
  adjustmentDate: string;
  storeId: string;
  totalAdjustmentQty?: number;
  status?: number;
  statusName: string;
  remark?: string;
  checkedBy: string;
  approvedBy: string;
  unpostedBy: string;
  createdOn: string;
  updatedOn: string;
  createdBy: string;
  updatedBy: string;
  store?: Store;
  stockAdjustmentDetails?: StockAdjustmentResponseDetail[];
}

export interface StockAdjustmentResponseDetail {
  id?: string;
  productId: string;
  adjustmentQty: number;
  currentStockQuantity?: number;
  product?: ProductView;
  createdOn: string;
  updatedOn: string;
  createdBy: string;
  updatedBy: string;
}
