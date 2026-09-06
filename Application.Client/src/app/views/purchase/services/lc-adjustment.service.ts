import { Injectable } from "@angular/core";
import { LcAdjustmentApiService } from "app/shared/api/purchase/lc-adjustment-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { Observable, map } from "rxjs";
import { LcAdjustmentRequestDTO } from "../models/lc-adjustment/lc-adjustment-request-dto.model";
import { LcAdjustmentResponseDTO } from "../models/lc-adjustment/lc-adjustment-response-dto.model";
import { LcAdjustmentSearchRequestDTO } from "../models/lc-adjustment/lc-adjustment-search-request-dto.model";
import { PendingCheckedCount } from "../models/pending-checked-count";

@Injectable({
  providedIn: "root",
})
export class LcAdjustmentService {
  constructor(private api: LcAdjustmentApiService) {}

  getLcAdjustments(
    lcAdjustmentRequest: LcAdjustmentSearchRequestDTO
  ): Observable<SearchResponse<LcAdjustmentResponseDTO>> {
    return this.api
      .getAll(lcAdjustmentRequest)
      .pipe(
        map((response: SearchResponse<LcAdjustmentResponseDTO>) => response)
      );
  }

  getLcAdjustmentById(
    id: string
  ): Observable<GeneralResponse<LcAdjustmentResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<LcAdjustmentResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createLcAdjustment(
    lcAdjustmentDTO: LcAdjustmentRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(lcAdjustmentDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateLcAdjustment(
    lcAdjustmentDTO: LcAdjustmentRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(lcAdjustmentDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteLcAdjustment(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkLcAdjustment(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveLcAdjustment(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostLcAdjustment(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
