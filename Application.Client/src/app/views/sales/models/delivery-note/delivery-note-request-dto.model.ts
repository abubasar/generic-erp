export interface DeliveryNoteRequestDTO {
  id?: string;
  saleOrderNo: string;
  orderDate: string;
  customerId: string;
  storeId?: string;
  deliveryDate: string;
  deliveryPlace: string;
  transport: number;
  creditLimit: number;
  limitAvailed: number;
  referenceNo: string;
  truckNo: string;
  driverName: string;
  driverContactNo: string;
  subtotal: number;
  discount: number;
  offerDiscount: number;
  otherDiscount: number;
  total: number;
  transportationCost: number;
  depoCharge: number;
  netTotal: number;
  remark: string;
  moneyReceiptNo: string;
  deliveryNoteDetails: DeliveryNoteRequestDetail[];
  deletedDeliveryNoteDetailIds?: string;
}

interface DeliveryNoteRequestDetail {
  id?: string;
  saleOrderDetailId?: string;
  productId: string;
  bagWeight: number;
  rate: number;
  netRate: number;
  discountPerUnit: number;
  offerDiscountPerUnit: number;
  invoiceDiscountPerUnit: number;
  cashDiscountPerUnit: number;
  specialDiscountPerUnit: number;
  discountAmount: number;
  amount: number;
  orderedPrimaryQuantity: number;
  orderedQuantity: number;
  orderedPrimaryBonusQuantity: number;
  orderedBonusQuantity: number;
  deliveryPrimaryQuantity: number;
  deliveryQuantity: number;
  deliveryPrimaryBonusQuantity: number;
  otherDiscountPerUnit: number;
  transportationCostPerUnit: number;
  depoChargePerUnit: number;
}
