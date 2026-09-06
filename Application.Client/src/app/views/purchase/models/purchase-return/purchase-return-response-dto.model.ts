import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";

export interface PurchaseReturnResponseDTO {
  id?: string;
  purchaseReturnNo: string;
  purchaseReturnDate: string;
  referenceNo: string;
  grnno: string;
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
  purchaseReturnDetails: PurchaseReturnResponseDetail[];
}

export interface PurchaseReturnResponseDetail {
  id?: string;
  purchaseReturnId?: string;
  productId: string;
  grnquantity?: number;
  grnBagWeightDeductionQty?: number;
  quantity: number;
  bagWeightDeductionQty: number;
  numberOfBagQuantity: number;
  netQuantity: number;
  rate: number;
  amount: number;
  product: ProductView;
}
