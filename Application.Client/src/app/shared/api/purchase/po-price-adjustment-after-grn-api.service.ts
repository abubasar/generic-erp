import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { PoPriceAdjustmentAfterGrnRequestDTO } from "app/views/purchase/models/po-price-adjustment-after-grn/po-price-adjustment-after-grn-request-dto.model";
import { PoPriceAdjustmentAfterGrnSearchRequestDTO } from "app/views/purchase/models/po-price-adjustment-after-grn/po-price-adjustment-after-grn-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class PoPriceAdjustmentAfterGrnApiService {
  baseURL = environment.apiURL + "/PoPriceAdjustmentAfterGrn";

  constructor(private httpClient: HttpClient) {}

  getAll(request: PoPriceAdjustmentAfterGrnSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: PoPriceAdjustmentAfterGrnSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(poPriceAdjustmentAfterGrn: PoPriceAdjustmentAfterGrnRequestDTO) {
    return this.httpClient.post(this.baseURL, poPriceAdjustmentAfterGrn);
  }

  update(poPriceAdjustmentAfterGrn: PoPriceAdjustmentAfterGrnRequestDTO) {
    return this.httpClient.post(
      this.baseURL + "/update",
      poPriceAdjustmentAfterGrn
    );
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
