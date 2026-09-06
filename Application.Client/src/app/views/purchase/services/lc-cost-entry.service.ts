import { Injectable } from "@angular/core";
import { LcCostEntryApiService } from "app/shared/api/purchase/lc-cost-entry-api.service";
import { GeneralResponse, GeneralResponse2 } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { Observable, map } from "rxjs";
import { LCCostEntryAggregatorModel } from "../models/lc-cost-entry/lc-cost-entry-aggregator.model";
import { LCCostEntryByPoIdResponseDTO } from "../models/lc-cost-entry/lc-cost-entry-by-po-id-response-dto.model";
import { LCCostEntryRequestDTO } from "../models/lc-cost-entry/lc-cost-entry-request-dto.model";
import { LCCostEntryResponseDTO } from "../models/lc-cost-entry/lc-cost-entry-response-dto.model";
import { LCCostEntrySearchRequestDTO } from "../models/lc-cost-entry/lc-cost-entry-search-request-dto.model";
import { PendingCheckedCount } from "../models/pending-checked-count";

@Injectable({
  providedIn: "root",
})
export class LcCostEntryService {
  constructor(private api: LcCostEntryApiService) {}

  getLCCostEntries(
    lcCostEntryRequest: LCCostEntrySearchRequestDTO
  ): Observable<SearchResponse<LCCostEntryResponseDTO>> {
    return this.api
      .getAll(lcCostEntryRequest)
      .pipe(
        map((response: SearchResponse<LCCostEntryResponseDTO>) => response)
      );
  }

  reportAggregates(
    lcCostEntryRequest: LCCostEntrySearchRequestDTO
  ): Observable<GeneralResponse<LCCostEntryAggregatorModel>> {
    return this.api
      .reportAggregates(lcCostEntryRequest)
      .pipe(
        map((response: GeneralResponse<LCCostEntryAggregatorModel>) => response)
      );
  }

  getLCCostEntryById(
    id: string
  ): Observable<GeneralResponse<LCCostEntryResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<LCCostEntryResponseDTO>) => response)
      );
  }

  getLcCostEntryByPurchaseOrderId(
    purchaseOrderId: string
  ): Observable<GeneralResponse2<LCCostEntryByPoIdResponseDTO>> {
    return this.api
      .getLcCostEntryByPoId(purchaseOrderId)
      .pipe(
        map(
          (response: GeneralResponse2<LCCostEntryByPoIdResponseDTO>) => response
        )
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createLCCostEntry(
    lcCostEntryDTO: LCCostEntryRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(lcCostEntryDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateLCCostEntry(
    lcCostEntryDTO: LCCostEntryRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(lcCostEntryDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteLCCostEntry(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkLCCostEntry(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveLCCostEntry(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostLCCostEntry(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
