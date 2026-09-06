import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { DeliveryNoteRequestDTO } from "app/views/sales/models/delivery-note/delivery-note-request-dto.model";
import { DeliveryNoteSearchRequestDTO } from "app/views/sales/models/delivery-note/delivery-note-search-request-dto.model";

import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class DeliveryNoteApiService {
  baseURL = environment.apiURL + "/DeliveryNote";

  constructor(private httpClient: HttpClient) {}

  getAll(request: DeliveryNoteSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: DeliveryNoteSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(saleOrder: DeliveryNoteRequestDTO) {
    return this.httpClient.post(this.baseURL, saleOrder);
  }

  update(saleOrder: DeliveryNoteRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", saleOrder);
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
