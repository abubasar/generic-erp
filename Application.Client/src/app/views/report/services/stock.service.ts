import { Injectable } from "@angular/core";
import { StockReportService } from "app/shared/api/report/stock-report.service";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { StockLedgerResponseDTO } from "../models/stock-ledger/stock-ledger-response-dto.model";
import { StockLedgerSearchRequestDTO } from "../models/stock-ledger/stock-ledger-search-request-dto.model";
import { StockResponseDTO } from "../models/stock-response-dto";
import { StockSearchRequestDTO } from "../models/stock-search-request-dto";

@Injectable({
  providedIn: "root",
})
export class StockService {
  constructor(private api: StockReportService) {}

  getStockData(
    stockSearchRequest: StockSearchRequestDTO
  ): Observable<SearchResponse<StockResponseDTO>> {
    return this.api
      .getAll(stockSearchRequest)
      .pipe(map((response: SearchResponse<StockResponseDTO>) => response));
  }
  getLowStockData(
    stockSearchRequest: StockSearchRequestDTO
  ): Observable<SearchResponse<StockResponseDTO>> {
    return this.api
      .getAllLowStock(stockSearchRequest)
      .pipe(map((response: SearchResponse<StockResponseDTO>) => response));
  }
  getWorkInProcessInventoryStockData(): Observable<StockResponseDTO[]> {
    return this.api
      .getAllWorkInProcessInventoryStock()
      .pipe(map((response: StockResponseDTO[]) => response));
  }
  getItemStock(productId: string, storeId: string): Observable<number> {
    return this.api
      .getItemStock(productId, storeId)
      .pipe(map((response: number) => response));
  }

  getStockLedger(
    stockLedger: StockLedgerSearchRequestDTO
  ): Observable<StockLedgerResponseDTO[]> {
    return this.api
      .getStockLedger(stockLedger)
      .pipe(map((response: StockLedgerResponseDTO[]) => response));
  }
}
