import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { StockLedgerSearchRequestDTO } from "app/views/report/models/stock-ledger/stock-ledger-search-request-dto.model";
import { StockSearchRequestDTO } from "app/views/report/models/stock-search-request-dto";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class StockReportService {
  baseURL = environment.apiURL + "/report";

  constructor(private httpClient: HttpClient) {}

  getAll(request: StockSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/stock", request);
  }
  getAllLowStock(request: StockSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/low-stock", request);
  }
  getAllWorkInProcessInventoryStock() {
    return this.httpClient.get(
      this.baseURL + "/work-in-process-inventory-stock"
    );
  }
  getItemStock(productId: string, storeId: string) {
    return this.httpClient.get(
      this.baseURL + `/stock-quantity/${productId}/${storeId}`
    );
  }

  getStockLedger(stockLedgerRequest: StockLedgerSearchRequestDTO) {
    return this.httpClient.post(
      this.baseURL + `/stock-ledger`,
      stockLedgerRequest
    );
  }
}
