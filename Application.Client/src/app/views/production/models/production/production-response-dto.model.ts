import { Machine } from "app/views/configuration/models/machine/machine.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Shift } from "app/views/configuration/models/shift/shift.model";
import { Store } from "app/views/configuration/models/store/store.model";

export interface ProductionResponseDTO {
  id?: string;
  productionNo?: string;
  manufacturingOrderNo?: string;
  bomNo?: string;
  extraDamageQuantity?: number;
  dustLooseInQuantity?: number;
  dustLooseOutQuantity?: number;
  fgstoreId?: string;
  totalRmused?: number;
  rmCost?: number;
  productionDate?: string;
  formulationNo?: string;
  batchNo?: string;
  finishedProductId?: string;
  productionQuantity?: number;
  actualProductionQuantity?: number;
  totalCost?: number;
  totalAdjustmentQuantity: number;
  shiftId: string;
  machineId: string;
  startDateTime?: string;
  endDateTime?: string;
  breakTime: number;
  status?: number;
  statusName?: string;
  remark?: string;
  checkedBy: string;
  approvedBy: string;
  createdOn: string;
  updatedOn: string;
  createdBy: string;
  updatedBy: string;
  financialYearId: string;
  finishedProduct?: ProductView;
  fgstore?: Store;
  machine?: Machine;
  shift?: Shift;
  productionDetails?: ProductionResponseDetail[];
}

export interface ProductionResponseDetail {
  id?: string;
  rawMaterialId: string;
  quantity: number;
  adjustmentQuantity: number;
  actualUsedQuantity: number;
  amount: number;
  rawMaterial?: ProductView;
}
