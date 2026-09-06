import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";

export interface ManufacturingOrderResponseDTO {
  id?: string;
  manufacturingOrderNo?: string;
  bomNo?: string;
  scheduledDate?: string;
  costCenterId?: string;
  formulationNo?: string;
  finishedProductId?: string;
  productionQuantity?: number;
  rawMaterialStoreId?: string;
  totalRmused?: number;
  rmCost?: number;
  standardDirectExpenseAmount?: number;
  standardFactoryOverheadAmount?: number;
  totalCost?: number;
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
  costCenter?: CostCenter;
  finishedProduct?: ProductView;
  rawMaterialStore?: Store;
  manufacturingOrderDetails?: ManufacturingOrderResponseDetail[];
}

export interface ManufacturingOrderResponseDetail {
  id?: string;
  rawMaterialId: string;
  stockQuantity: number;
  percentage: number;
  quantity: number;
  amount: number;
  rawMaterial?: ProductView;
}
