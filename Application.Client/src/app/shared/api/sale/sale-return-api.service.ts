import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { SaleReturnRequestDTO } from "app/views/sales/models/sale-return/sale-return-request-dto.model";
import { SaleReturnSearchRequestDTO } from "app/views/sales/models/sale-return/sale-return-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class SaleReturnApiService {
  baseURL = environment.apiURL + "/SaleReturn";

  constructor(private httpClient: HttpClient) {}

  getAll(request: SaleReturnSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: SaleReturnSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(saleReturn: SaleReturnRequestDTO) {
    return this.httpClient.post(this.baseURL, saleReturn);
  }

  update(saleReturn: SaleReturnRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", saleReturn);
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
