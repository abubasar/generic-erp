import { Product } from "app/shared/models/product.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
export interface PurchaseRequisitionRequestDTO {
  id?: string;
  requisitionNo?: string;
  requisitionDate?: string;
  storeId?: string;
  departmentId?: string;
 // totalAmount?: number;
  expectedDeliveryDate?: string;
  priority?: number;
  priorityName?: string;
  paymentTermInDays: 0;
  transport: 0;
  paymentMode:0,
  currencyId: string;
  importPurchaseIncoTerm: number;
  importPurchasePaymentTerm: number;
  termAndCondition: string;
  remark?: string;
  deletedPurchaseRequisitionDetailIds?: string;
  purchaseRequisitionDetails?: PurchaseRequisitionRequestDetail[];
}

export interface PurchaseRequisitionRequestDetail {
  id?: string;
  productId?: string;
  product?: ProductView; //! Don't remove this product
  quantity?: number;
  // rate?: number;
  //amount?: number;
  lastPurchaseRate?: number;
  orderedQuantity?: number;
  alertQuantity?: number;
  currentStockQuantity?: number;
}
