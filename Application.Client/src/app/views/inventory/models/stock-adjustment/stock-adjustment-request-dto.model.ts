import { ProductView } from "app/views/configuration/models/product/product-view.model";

export interface StockAdjustmentRequestDTO {
    id?: string;
    code?: string;
    adjustmentDate: string;
    storeId: string;
    totalAdjustmentQty: number;
    remark?: string;
    deletedStockAdjustmentDetailIds?: string;
    stockAdjustmentDetails?: StockAdjustmentRequestDetail[];
  }
  
  export interface StockAdjustmentRequestDetail {
    id?: string;
    productId: string;
    product?: ProductView;
    adjustmentQty: number;
  }