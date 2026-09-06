import { Injectable } from "@angular/core";
import { GoodsReceiveNoteApiService } from "app/shared/api/purchase/goods-receive-note-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { GoodsReceiveNoteAggregatorModel } from "../models/goods-receive-note/goods-receive-note-aggregator.model";
import { GoodsReceiveNoteRequestDTO } from "../models/goods-receive-note/goods-receive-note-request-dto.model";
import { GoodsReceiveNoteResponseDTO } from "../models/goods-receive-note/goods-receive-note-response-dto.model";
import { GoodsReceiveNoteSearchRequestDTO } from "../models/goods-receive-note/goods-receive-note-search-request-dto.model";
import { PendingCheckedCount } from "../models/pending-checked-count";

@Injectable({
  providedIn: "root",
})
export class GoodsReceiveNoteService {
  constructor(private api: GoodsReceiveNoteApiService) {}

  getGoodsReceiveNotes(
    goodsReceiveNoteRequest: GoodsReceiveNoteSearchRequestDTO
  ): Observable<SearchResponse<GoodsReceiveNoteResponseDTO>> {
    return this.api
      .getAll(goodsReceiveNoteRequest)
      .pipe(
        map((response: SearchResponse<GoodsReceiveNoteResponseDTO>) => response)
      );
  }

  reportAggregates(
    goodsReceiveNoteRequest: GoodsReceiveNoteSearchRequestDTO
  ): Observable<GeneralResponse<GoodsReceiveNoteAggregatorModel>> {
    return this.api
      .reportAggregates(goodsReceiveNoteRequest)
      .pipe(
        map(
          (response: GeneralResponse<GoodsReceiveNoteAggregatorModel>) =>
            response
        )
      );
  }

  getGoodsReceiveNoteById(
    id: string
  ): Observable<GeneralResponse<GoodsReceiveNoteResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map(
          (response: GeneralResponse<GoodsReceiveNoteResponseDTO>) => response
        )
      );
  }
  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createGoodsReceiveNote(
    goodsReceiveNoteDTO: GoodsReceiveNoteRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(goodsReceiveNoteDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateGoodsReceiveNote(
    goodsReceiveNoteDTO: GoodsReceiveNoteRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(goodsReceiveNoteDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteGoodsReceiveNote(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkGoodsReceiveNote(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveGoodsReceiveNote(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostGoodsReceiveNote(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
