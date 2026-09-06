import { Injectable } from "@angular/core";
import { PoPriceAdjustmentAfterGrnApiService } from "app/shared/api/purchase/po-price-adjustment-after-grn-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { Observable, map } from "rxjs";
import { PendingCheckedCount } from "../models/pending-checked-count";
import { PoPriceAdjustmentAfterGrnAggregatorModel } from "../models/po-price-adjustment-after-grn/po-price-adjustment-after-grn-aggregator.model";
import { PoPriceAdjustmentAfterGrnRequestDTO } from "../models/po-price-adjustment-after-grn/po-price-adjustment-after-grn-request-dto.model";
import { PoPriceAdjustmentAfterGrnResponseDTO } from "../models/po-price-adjustment-after-grn/po-price-adjustment-after-grn-response-dto.model";
import { PoPriceAdjustmentAfterGrnSearchRequestDTO } from "../models/po-price-adjustment-after-grn/po-price-adjustment-after-grn-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class PoPriceAdjustmentAfterGrnService {
  constructor(private api: PoPriceAdjustmentAfterGrnApiService) {}

  getPoPriceAdjustmentAfterGrns(
    poPriceAdjustmentAfterGrnRequest: PoPriceAdjustmentAfterGrnSearchRequestDTO
  ): Observable<SearchResponse<PoPriceAdjustmentAfterGrnResponseDTO>> {
    return this.api
      .getAll(poPriceAdjustmentAfterGrnRequest)
      .pipe(
        map(
          (response: SearchResponse<PoPriceAdjustmentAfterGrnResponseDTO>) =>
            response
        )
      );
  }

  reportAggregates(
    poPriceAdjustmentAfterGrnRequest: PoPriceAdjustmentAfterGrnSearchRequestDTO
  ): Observable<GeneralResponse<PoPriceAdjustmentAfterGrnAggregatorModel>> {
    return this.api
      .reportAggregates(poPriceAdjustmentAfterGrnRequest)
      .pipe(
        map(
          (
            response: GeneralResponse<PoPriceAdjustmentAfterGrnAggregatorModel>
          ) => response
        )
      );
  }

  getPoPriceAdjustmentAfterGrnById(
    id: string
  ): Observable<GeneralResponse<PoPriceAdjustmentAfterGrnResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map(
          (response: GeneralResponse<PoPriceAdjustmentAfterGrnResponseDTO>) =>
            response
        )
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createPoPriceAdjustmentAfterGrn(
    poPriceAdjustmentAfterGrnDTO: PoPriceAdjustmentAfterGrnRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(poPriceAdjustmentAfterGrnDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updatePoPriceAdjustmentAfterGrn(
    poPriceAdjustmentAfterGrnDTO: PoPriceAdjustmentAfterGrnRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(poPriceAdjustmentAfterGrnDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deletePoPriceAdjustmentAfterGrn(
    id: string
  ): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkPoPriceAdjustmentAfterGrn(
    id: string
  ): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approvePoPriceAdjustmentAfterGrn(
    id: string
  ): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostPoPriceAdjustmentAfterGrn(
    id: string,
    status:number
  ): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
