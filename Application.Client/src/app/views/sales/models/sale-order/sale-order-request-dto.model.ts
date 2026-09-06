import { ProductView } from "app/views/configuration/models/product/product-view.model";

export interface SaleOrderRequestDTO {
  id?: string;
  quotationNo: string;
  transport: number;
  orderDate: string;
  customerId: string;
  creditLimit: number;
  limitAvailed: number;
  deliveryDate: string;
  referenceNo: string;
  storeId: string;
  subtotal: number;
  discount: number;
  totalPercentageDiscountAmount: number;
  offerDiscount: number;
  otherDiscount: number;
  total: number;
  transportationCost: number;
  depoCharge: number;
  netTotal: number;
  isMailSent: boolean;
  remark: string;
  paymentTerm: number;
  customerTerritoryId: string;
  moneyReceiptNo: string;
  customerWiseProductDiscountId: string;
  discountProductWiseId: string;
  saleOrderDetails: SaleOrderRequestDetail[];
  deletedSaleOrderDetailIds?: string;
}

interface SaleOrderRequestDetail {
  id?: string;
  productId: string;
  product?: ProductView;
  measurementUnitId?: string;
  bagWeight: number;
  primaryQuantity: number;
  primaryBonusQuantity: number;
  quantity: number;
  bonusQuantity: number;
  deliveredPrimaryQuantity?: number;
  deliveredPrimaryBonusQuantity?: number;
  rate: number;
  vatPercentage: number;
  discountPercentage: number;
  percentageDiscountAmount: number;
  netRate: number;
  discountAmount: number;
  discountPerUnit: number;
  offerDiscountPerUnit: number;
  invoiceDiscountPerUnit: number;
  cashDiscountPerUnit: number;
  specialDiscountPerUnit: number;
  amount: number;
  currentStockQuantity: number;
}
