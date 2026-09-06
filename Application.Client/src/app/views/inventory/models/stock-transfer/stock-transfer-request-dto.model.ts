export interface StockTransferRequestDTO {
  id?: string;
  sourceId: string;
  destinationId: string;
  transferDate: string;
  truckNo: string;
  remark: string;
  stockTransferDetails: StockTransferRequestDetail[];
  deletedStockTransferDetailIds?: "string";
}

interface StockTransferRequestDetail {
  id?: string;
  productId: string;
  transferBagQuantity: number;
  transferQuantity: number;
}
