export interface ReceivePaymentAgainstSaleRequestDTO {
  id?: string;
  paymentDate: string;
  invoiceNo: string;
  customerId: string;
  amount: number;
  costCenterId: string;
  toAccountId: string;
  remark: string;
}
