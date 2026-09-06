import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { JournalEntryRequestDTO } from "app/views/accounts/models/journal-entry/journal-entry-request-dto.model";
import { JournalEntrySearchRequestDTO } from "app/views/accounts/models/journal-entry/journal-entry-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class JournalEntryApiService {
  baseURL = environment.apiURL + "/JournalEntry";

  constructor(private httpClient: HttpClient) {}

  getAll(request: JournalEntrySearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(journalEntry: JournalEntryRequestDTO) {
    return this.httpClient.post(this.baseURL, journalEntry);
  }

  update(journalEntry: JournalEntryRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", journalEntry);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }

  check(id: string) {
    return this.httpClient.post(this.baseURL + `/check/${id}`, {});
  }

  approve(id: string) {
    return this.httpClient.post(this.baseURL + `/approve/${id}`, {});
  }

  unpost(id: string, status: number) {
    return this.httpClient.post(this.baseURL + `/unpost/${id}/${status}`, {});
  }
}
