import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";

export interface StockTransferResponseDTO {
  id: string;
  sourceId: string;
  destinationId: string;
  transferNo: string;
  transferDate: string;
  truckNo: string;
  status: number;
  statusName: string;
  remark: string;
  checkedBy: string;
  approvedBy: string;
  createdOn: string;
  updatedOn: string;
  createdBy: string;
  updatedBy: string;
  financialYearId: string;
  source: Store;
  destination: Store;
  stockTransferDetails: StockTransferResponseDetail[];
}

export interface StockTransferResponseDetail {
  id?: string;
  stockTransferId: string;
  productId: string;
  // bagWeight: string;
  transferBagQuantity: number;
  transferQuantity: number;
  product: ProductView;
  currentStockQuantity?: number;
}
