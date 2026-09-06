import { ProductView } from "app/views/configuration/models/product/product-view.model";

export interface ManufacturingOrderRequestDTO {
  id?: string;
  manufacturingOrderNo?: string;
  bomNo: string;
  scheduledDate: string;
  costCenterId: string;
  formulationNo: string;
  finishedProductId?: string;
  productionQuantity: number;
  rawMaterialStoreId: string;
  totalRmused: number;
  rmCost: number;
  standardDirectExpenseAmount: number;
  standardFactoryOverheadAmount:number;
  totalCost: number;
  remark?: string;
  deletedManufacturingOrderDetailIds?: string;
  manufacturingOrderDetails?: ManufacturingOrderRequestDetail[];
}

export interface ManufacturingOrderRequestDetail {
  id?: string;
  rawMaterialId: string;
  rawMaterial?: ProductView;
  stockQuantity: number;
  percentage: number;
  quantity: number;
  amount?: number;
}
