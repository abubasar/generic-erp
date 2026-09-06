import { Injectable } from "@angular/core";
import { PurchaseReturnApiService } from "app/shared/api/purchase/purchase-return-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { Observable, map } from "rxjs";
import { PendingCheckedCount } from "../models/pending-checked-count";
import { PurchaseReturnAggregatorModel } from "../models/purchase-return/purchase-return-aggregator.model";
import { PurchaseReturnRequestDTO } from "../models/purchase-return/purchase-return-request-dto.model";
import { PurchaseReturnResponseDTO } from "../models/purchase-return/purchase-return-response-dto.model";
import { PurchaseReturnSearchRequestDTO } from "../models/purchase-return/purchase-return-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class PurchaseReturnService {
  constructor(private api: PurchaseReturnApiService) {}

  getPurchaseReturns(
    saleReturnRequest: PurchaseReturnSearchRequestDTO
  ): Observable<SearchResponse<PurchaseReturnResponseDTO>> {
    return this.api
      .getAll(saleReturnRequest)
      .pipe(
        map((response: SearchResponse<PurchaseReturnResponseDTO>) => response)
      );
  }

  reportAggregates(
    purchaseReturnRequest: PurchaseReturnSearchRequestDTO
  ): Observable<GeneralResponse<PurchaseReturnAggregatorModel>> {
    return this.api
      .reportAggregates(purchaseReturnRequest)
      .pipe(
        map(
          (response: GeneralResponse<PurchaseReturnAggregatorModel>) => response
        )
      );
  }

  getPurchaseReturnById(
    id: string
  ): Observable<GeneralResponse<PurchaseReturnResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<PurchaseReturnResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createPurchaseReturn(
    saleReturnDTO: PurchaseReturnRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(saleReturnDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updatePurchaseReturn(
    saleReturnDTO: PurchaseReturnRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(saleReturnDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deletePurchaseReturn(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkPurchaseReturn(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approvePurchaseReturn(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostPurchaseReturn(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
