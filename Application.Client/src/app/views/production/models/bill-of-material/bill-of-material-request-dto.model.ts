import { ProductView } from "app/views/configuration/models/product/product-view.model";

export interface BillOfMaterialRequestDTO {
  id?: string;
  bomNo?: string;
  copiedFromBomNo?: string;
  finishedProductId?: string;
  formulationNo: string;
  dosageQuantity: number;
  totalQuantity: number;
  remark?: string;
  deletedBillOfMaterialDetailIds?: string;
  billOfMaterialDetails?: BillOfMaterialRequestDetail[];
}

export interface BillOfMaterialRequestDetail {
  id?: string;
  rawMaterialId: string;
  rawMaterial?: ProductView;
  quantity: number;
  percentage?: number;
}
