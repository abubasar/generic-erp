export interface StockResponseDTO {
  productTypeId?: string;
  productId?: string;
  storeId?: string;
  availableQty: number;
  stockValue: number;
  productName?: string;
  storeName?: string;
}
