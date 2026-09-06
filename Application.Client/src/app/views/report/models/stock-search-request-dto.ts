export class StockSearchRequestDTO {
  constructor() {
    this.page = 0;
    this.rowsPerPage = 5;
  }

  page: number;
  rowsPerPage: number;
  productId?: string;
  inventoryTypeId?: string;
  productTypeId?: string;
  storeId?: string;
}
