export class StockLedgerSearchRequestDTO {
  fromDate: string;
  toDate: string;
  storeId?: string;
  productId?: string;
  inventoryTypeId?:string;
}
