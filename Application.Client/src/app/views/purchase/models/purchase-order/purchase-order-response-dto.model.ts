import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { Currency } from "app/views/configuration/models/currency/currency.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";
import { Supplier } from "app/views/configuration/models/supplier/supplier.model";

export interface PurchaseOrderResponseDTO {
  id?: string;
  requisitionNo: string;
  quotationNo: string;
  referenceNo: string;
  ponumber: string;
  podate: string;
  deliveryPlaceId: string;
  paymentMethodId: string;
  storeId: string;
  supplierId: string;
  productOrigin: string;
  packagingType: string;
  expiryTime: string;
  transport: number;
  paymentTermInDays: number;
  deliveryTermInDays: number;
  deliveryDate: string;
  paymentMode: number;
  paymentModeName: string;
  weightVariance: number;
  status: number;
  statusName: string;
  isImportPurchase: boolean;
  proformaInvoiceNo: string;
  lcNumber: string;
  exchangeRate: number;
  currencyId: string;
  importPurchaseIncoTerm: number;
  importPurchasePaymentTerm: number;
  portOfLoading: string;
  portOfDestination: string;
  termAndCondition: string;
  subtotal: number;
  discount: number;
  total: number;
  remark: string;
  isPartialDelivery: boolean;
  createdOn: string;
  updatedOn: string;
  createdBy: string;
  updatedBy: string;
  checkedBy: string;
  approvedBy: string;
  financialYearId: string;
  costCenter: CostCenter;
  store: Store;
  supplier: Supplier;
  currency: Currency;
  purchaseOrderDetails: PurchaseOrderResponseDetail[];
}

export interface PurchaseOrderResponseDetail {
  id?: string;
  // purchaseOrderId?: string;
  productId: string;
  lastPoDetails?: string;
  poId?: string;
  lastPoStatus?: number;
  measurementUnitName?: string;
  quantity: number;
  receivedQuantity?: number;
  currencyRate: number;
  rate: number;
  currencyAmount: number;
  amount: number;
  product?: ProductView;
}

export interface LastPoDetails {
  poId: string;
  productId: string;
  productName: string;
  poNumber: string;
  orderedQuantity: number;
  receivedQuantity: number;
  rate: number;
  status: number;
}
