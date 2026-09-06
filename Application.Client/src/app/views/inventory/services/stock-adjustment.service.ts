import { Injectable } from "@angular/core";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { Observable, map } from "rxjs";
import { StockAdjustmentRequestDTO } from "../models/stock-adjustment/stock-adjustment-request-dto.model";
import { StockAdjustmentResponseDTO } from "../models/stock-adjustment/stock-adjustment-response-dto.model";
import { StockAdjustmentSearchRequestDTO } from "../models/stock-adjustment/stock-adjustment-search-request-dto.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { StockAdjustmentApiService } from "app/shared/api/inventory/stock-adjustment-api.service";

@Injectable({
  providedIn: "root",
})
export class StockAdjustmentService {
  constructor(private api: StockAdjustmentApiService) {}

  getStockAdjustments(
    stockAdjustmentRequest: StockAdjustmentSearchRequestDTO
  ): Observable<SearchResponse<StockAdjustmentResponseDTO>> {
    return this.api
      .getAll(stockAdjustmentRequest)
      .pipe(
        map((response: SearchResponse<StockAdjustmentResponseDTO>) => response)
      );
  }

  getStockAdjustmentById(
    id: string
  ): Observable<GeneralResponse<StockAdjustmentResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<StockAdjustmentResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }
  createStockAdjustment(
    stockAdjustmentDTO: StockAdjustmentRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(stockAdjustmentDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateStockAdjustment(
    stockAdjustmentDTO: StockAdjustmentRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(stockAdjustmentDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteStockAdjustment(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkStockAdjustment(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveStockAdjustment(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
  unpostStockAdjustment(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
