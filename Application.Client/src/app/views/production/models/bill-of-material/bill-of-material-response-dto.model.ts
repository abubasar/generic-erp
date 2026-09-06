import { ProductView } from "app/views/configuration/models/product/product-view.model";

export interface BillOfMaterialResponseDTO {
  id?: string;
  bomNo?: string;
  copiedFromBomNo?: string;
  finishedProductId?: string;
  formulationNo?: string;
  dosageQuantity?: number;
  totalQuantity: number;
  status?: number;
  statusName: string;
  remark?: string;
  checkedBy: string;
  approvedBy: string;
  createdOn: string;
  updatedOn: string;
  createdBy: string;
  updatedBy: string;
  financialYearId: string;
  finishedProduct?: ProductView;
  billOfMaterialDetails?: BillOfMaterialResponseDetail[];
}

export interface BillOfMaterialResponseDetail {
  id?: string;
  rawMaterialId: string;
  quantity: number;
  percentage: number;
  rawMaterial?: ProductView;
}
