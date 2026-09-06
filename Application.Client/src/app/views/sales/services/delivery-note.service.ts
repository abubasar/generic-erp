import { Injectable } from "@angular/core";
import { DeliveryNoteApiService } from "app/shared/api/sale/delivery-note-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { Observable, map } from "rxjs";
import { DeliveryNoteAggregatorModel } from "../models/delivery-note/delivery-note-aggregator.model";
import { DeliveryNoteRequestDTO } from "../models/delivery-note/delivery-note-request-dto.model";
import { DeliveryNoteResponseDTO } from "../models/delivery-note/delivery-note-response-dto.model";
import { DeliveryNoteSearchRequestDTO } from "../models/delivery-note/delivery-note-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class DeliveryNoteService {
  constructor(private api: DeliveryNoteApiService) {}

  getDeliveryNotes(
    deliveryNoteRequest: DeliveryNoteSearchRequestDTO
  ): Observable<SearchResponse<DeliveryNoteResponseDTO>> {
    return this.api
      .getAll(deliveryNoteRequest)
      .pipe(
        map((response: SearchResponse<DeliveryNoteResponseDTO>) => response)
      );
  }

  reportAggregates(
    deliveryNoteRequest: DeliveryNoteSearchRequestDTO
  ): Observable<GeneralResponse<DeliveryNoteAggregatorModel>> {
    return this.api
      .reportAggregates(deliveryNoteRequest)
      .pipe(
        map(
          (response: GeneralResponse<DeliveryNoteAggregatorModel>) => response
        )
      );
  }

  getDeliveryNoteById(
    id: string
  ): Observable<GeneralResponse<DeliveryNoteResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<DeliveryNoteResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createDeliveryNote(
    deliveryNoteDTO: DeliveryNoteRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(deliveryNoteDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateDeliveryNote(
    deliveryNoteDTO: DeliveryNoteRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(deliveryNoteDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteDeliveryNote(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkDeliveryNote(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveDeliveryNote(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostDeliveryNote(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
