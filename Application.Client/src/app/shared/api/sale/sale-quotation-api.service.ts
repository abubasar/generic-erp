import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { SaleQuotationRequestDTO } from "app/views/sales/models/sale-quotation/sale-quotation-request-dto.model";
import { SaleQuotationSearchRequestDTO } from "app/views/sales/models/sale-quotation/sale-quotation-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class SaleQuotationApiService {
  baseURL = environment.apiURL + "/saleQuotation";

  constructor(private httpClient: HttpClient) {}

  getAll(request: SaleQuotationSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: SaleQuotationSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(saleQuotation: SaleQuotationRequestDTO) {
    return this.httpClient.post(this.baseURL, saleQuotation);
  }

  update(saleQuotation: SaleQuotationRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", saleQuotation);
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
