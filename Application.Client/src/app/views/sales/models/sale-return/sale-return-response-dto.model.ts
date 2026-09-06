import { Customer } from "app/views/configuration/models/customer/customer.model";
import { ProductView } from "app/views/configuration/models/product/product-view.model";
import { Store } from "app/views/configuration/models/store/store.model";

export interface SaleReturnResponseDTO {
  id?: string;
  deliveryNoteNo?: string;
  invoiceNo?: string;
  saleReturnNo: string;
  saleReturnDate: string;
  referenceNo: string;
  storeId: string;
  customerId: string;
  customerMarketingOfficerId: string;
  customerTerritoryId: string;
  subtotal: number;
  totalPercentageDiscountAmount: number;
  otherDiscount: number;
  total: number;
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
  customer: Customer;
  store: Store;
  saleReturnDetails: SaleReturnResponseDetail[];
}

export interface SaleReturnResponseDetail {
  id?: string;
  saleReturnId?: string;
  productId: string;
  bagWeight: number;
  primaryQuantity: number;
  quantity: number;
  primaryBonusQuantity: number;
  bonusQuantity: number;
  rate: number;
  returnPrimaryQuantity: number;
  returnQuantity: number;
  returnPrimaryBonusQuantity: number;
  returnBonusQuantity: number;
  vatPercentage: number;
  discountPercentage: number;
  percentageDiscountAmount: number;
  otherDiscountPerUnit: number;
  amount: number;
  product: ProductView;
  createdOn?: string;
  createdBy?: string;
}
