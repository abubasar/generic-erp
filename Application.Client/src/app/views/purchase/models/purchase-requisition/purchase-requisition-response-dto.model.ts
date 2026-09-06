import { Currency } from "app/views/configuration/models/currency/currency.model";
import { Department } from "app/views/configuration/models/department/department.model";
import { MeasurementUnit } from "app/views/configuration/models/measurement-unit/measurement-unit.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";

export interface PurchaseRequisitionResponseDTO {
  id?: string;
  requisitionNo?: string;
  requisitionDate?: string;
  storeId?: string;
  departmentId?: string;
  totalAmount?: number;
  expectedDeliveryDate?: string;
  requestBy?: string;
  requestByName?: string;
  priority?: number;
  priorityName?: string;
  paymentTermInDays: 0;
  transport: 0;
  paymentMode: 0;
  currencyId: string;
  importPurchaseIncoTerm: number;
  importPurchasePaymentTerm: number;
  termAndCondition: string;
  remark?: string;
  requisitionStatus?: number;
  requisitionStatusName?: string;
  createdOn: string;
  updatedOn: string;
  createdBy: string;
  updatedBy: string;
  checkedBy: string;
  approvedBy: string;
  financialYearId: string;
  department?: Department;
  store?: Store;
  currency?: Currency;
  purchaseRequisitionDetails?: PurchaseRequisitionResponseDetail[];
}

export interface PurchaseRequisitionResponseDetail {
  id: string;
  // purchaseRequisitionId?: string;
  productId: string;
  measurementUnitId?: string;
  quantity: number;
  rate: number;
  amount: number;
  lastPurchaseRate?: number;
  orderedQuantity?: number;
  alertQuantity?: number;
  currentStockQuantity?: number;
  measurementUnit?: MeasurementUnit;
  product?: ProductView;
}
