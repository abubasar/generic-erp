import { Injectable } from "@angular/core";
import { JournalEntryApiService } from "app/shared/api/account/journal-entry-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { Observable, map } from "rxjs";
import { JournalEntryRequestDTO } from "../models/journal-entry/journal-entry-request-dto.model";
import { JournalEntryResponseDTO } from "../models/journal-entry/journal-entry-response-dto.model";
import { JournalEntrySearchRequestDTO } from "../models/journal-entry/journal-entry-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class JournalEntryService {
  constructor(private api: JournalEntryApiService) {}

  getJournalEntries(
    journalEntryRequest: JournalEntrySearchRequestDTO
  ): Observable<SearchResponse<JournalEntryResponseDTO>> {
    return this.api
      .getAll(journalEntryRequest)
      .pipe(
        map((response: SearchResponse<JournalEntryResponseDTO>) => response)
      );
  }

  getJournalEntryById(
    id: string
  ): Observable<GeneralResponse<JournalEntryResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<JournalEntryResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createJournalEntry(
    journalEntryDTO: JournalEntryRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(journalEntryDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateJournalEntry(
    journalEntryDTO: JournalEntryRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(journalEntryDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteJournalEntry(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkJournalEntry(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveJournalEntry(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostJournalEntry(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
