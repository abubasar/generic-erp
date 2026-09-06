export interface CustomerDiscountReportResponseDTO {
  customerId: string;
  customerName: string;
  productName: string;
  productId: string;
  totalQuantity: number;
  monthlyDiscount: number;
  yearlyDiscount: number;
  targetDiscount: number;
}

