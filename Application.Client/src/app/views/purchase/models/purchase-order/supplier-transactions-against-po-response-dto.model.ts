export interface SupplierTransactionsAgainstPOResponseDTO {
  transactionDate: string;
  ponumber: string;
  supplierName: string;
  supplierTransactionTypeName: string;
  supplierInvoiceDate: string;
  paymentTermInDays: number;
  amount: number;
  balance: number;
}
