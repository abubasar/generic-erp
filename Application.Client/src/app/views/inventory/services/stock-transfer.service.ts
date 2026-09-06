import { Injectable } from "@angular/core";
import { StockTransferApiService } from "app/shared/api/inventory/stock-transfer-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { Observable, map } from "rxjs";
import { StockTransferRequestDTO } from "../models/stock-transfer/stock-transfer-request-dto.model";
import { StockTransferResponseDTO } from "../models/stock-transfer/stock-transfer-response-dto.model";
import { StockTransferSearchRequestDTO } from "../models/stock-transfer/stock-transfer-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class StockTransferService {
  constructor(private api: StockTransferApiService) {}

  getStockTransfers(
    stockTransferRequest: StockTransferSearchRequestDTO
  ): Observable<SearchResponse<StockTransferResponseDTO>> {
    return this.api
      .getAll(stockTransferRequest)
      .pipe(
        map((response: SearchResponse<StockTransferResponseDTO>) => response)
      );
  }

  getStockTransferById(
    id: string
  ): Observable<GeneralResponse<StockTransferResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<StockTransferResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createStockTransfer(
    stockTransferDTO: StockTransferRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(stockTransferDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateStockTransfer(
    stockTransferDTO: StockTransferRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(stockTransferDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteStockTransfer(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkStockTransfer(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveStockTransfer(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostStockTransfer(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
