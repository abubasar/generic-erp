import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { CustomerDiscountReportSearchRequestDTO } from "app/views/report/models/customer-monthly-discount-report/customer-discount-report-search-request-dto";
import { SaleInvoiceRequestDTO } from "app/views/sales/models/sale-invoice/sale-invoice-request-dto.model";
import { SaleInvoiceSearchRequestDTO } from "app/views/sales/models/sale-invoice/sale-invoice-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class SaleInvoiceApiService {
  baseURL = environment.apiURL + "/saleInvoice";

  constructor(private httpClient: HttpClient) {}

  getAll(request: SaleInvoiceSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: SaleInvoiceSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(saleInvoice: SaleInvoiceRequestDTO) {
    return this.httpClient.post(this.baseURL, saleInvoice);
  }

  update(saleInvoice: SaleInvoiceRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", saleInvoice);
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

  send(id: string) {
    return this.httpClient.post(this.baseURL + `/send/` + id, {});
  }

  unpost(id: string, status: number) {
    return this.httpClient.post(this.baseURL + `/unpost/${id}/${status}`, {});
  }

  getCustomerMonthlyDiscountReport(
    request: CustomerDiscountReportSearchRequestDTO
  ) {
    return this.httpClient.post(
      this.baseURL + "/customer-discount-report",
      request
    );
  }
}
