import { ProductView } from "app/views/configuration/models/product/product-view.model";

export interface ProductionRequestDTO {
  id?: string;
  productionNo?: string;
  manufacturingOrderNo?: string;
  bomNo: string;
  extraDamageQuantity: number;
  dustLooseInQuantity: number;
  dustLooseOutQuantity: number;
  fgstoreId: string;
  totalRmused: number;
  rmCost: number;
  productionDate: string;
  formulationNo: string;
  batchNo: string;
  finishedProductId?: string;
  productionQuantity: number;
  actualProductionQuantity: number;
  totalCost: number;
  totalAdjustmentQuantity: number;
  shiftId: string;
  machineId: string;
  startDateTime?: string;
  endDateTime?: string;
  breakTime: number;
  remark?: string;
  deletedProductionDetailIds?: string;
  productionDetails?: ProductionRequestDetail[];
}

export interface ProductionRequestDetail {
  id?: string;
  rawMaterialId: string;
  rawMaterial?: ProductView;
  quantity: number;
  adjustmentQuantity: number;
  actualUsedQuantity: number;
  amount?: number;
}
