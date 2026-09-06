import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";

export interface PoPriceAdjustmentAfterGrnResponseDTO {
  id?: string;
  code: string;
  adjustmentDate: string;
  referenceNo: string;
  grnno: string;
  ponumber: string;
  storeId: string;
  supplierId: string;
  totalAmount: number;
  remark: string;
  status: number;
  statusName: string;
  checkedBy: string;
  approvedBy: string;
  createdOn: string;
  updatedOn: string;
  createdBy: string;
  updatedBy: string;
  financialYearId: string;
  supplier: Supplier;
  store: Store;
  poPriceAdjustmentAfterGrnDetails: PoPriceAdjustmentAfterGrnResponseDetail[];
}

export interface PoPriceAdjustmentAfterGrnResponseDetail {
  id?: string;
  poPriceAdjustmentAfterGrnId?: string;
  productId: string;
  grnQuantity?: number;
  grnRate?: number;
  adjustmentQuantity: number;
  adjustmentRate: number;
  amount: number;
  product: ProductView;
}
