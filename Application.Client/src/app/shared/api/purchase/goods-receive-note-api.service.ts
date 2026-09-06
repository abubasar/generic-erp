import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { GoodsReceiveNoteRequestDTO } from "app/views/purchase/models/goods-receive-note/goods-receive-note-request-dto.model";
import { GoodsReceiveNoteSearchRequestDTO } from "app/views/purchase/models/goods-receive-note/goods-receive-note-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class GoodsReceiveNoteApiService {
  baseURL = environment.apiURL + "/goodsReceiveNote";

  constructor(private httpClient: HttpClient) {}

  getAll(request: GoodsReceiveNoteSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: GoodsReceiveNoteSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(goodsReceiveNote: GoodsReceiveNoteRequestDTO) {
    return this.httpClient.post(this.baseURL, goodsReceiveNote);
  }

  update(goodsReceiveNote: GoodsReceiveNoteRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", goodsReceiveNote);
  }

  delete(id: string) {
    return this.httpClient.post(this.baseURL + `/delete/${id}`, {});
  }

  check(id: string) {
    return this.httpClient.post(this.baseURL + `/check/` + id, {});
  }

  approve(id: string) {
    return this.httpClient.post(this.baseURL + `/approve/` + id, {});
  }

  unpost(id: string, status: number) {
    return this.httpClient.post(this.baseURL + `/unpost/${id}/${status}`, {});
  }

  // verify(goodsReceiveNoteIds: string[]) {
  //   return this.httpClient.post(this.baseURL + `/verify`, goodsReceiveNoteIds);
  // }
}
