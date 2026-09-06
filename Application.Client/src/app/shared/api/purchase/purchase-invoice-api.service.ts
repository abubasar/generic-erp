import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { PurchaseInvoiceRequestDTO } from "app/views/purchase/models/purchase-invoice/purchase-invoice-request-dto.model";
import { PurchaseInvoiceSearchRequestDTO } from "app/views/purchase/models/purchase-invoice/purchase-invoice-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class PurchaseInvoiceApiService {
  baseURL = environment.apiURL + "/purchaseInvoice";

  constructor(private httpClient: HttpClient) {}

  getAll(request: PurchaseInvoiceSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: PurchaseInvoiceSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(purchaseInvoice: PurchaseInvoiceRequestDTO) {
    return this.httpClient.post(this.baseURL, purchaseInvoice);
  }

  update(purchaseInvoice: PurchaseInvoiceRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", purchaseInvoice);
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

  resetAdvanceAmount(id: string) {
    return this.httpClient.post(
      this.baseURL + `/reset-advance-amount/` + id,
      {}
    );
  }

  // verify(purchaseInvoiceIds: string[]) {
  //   return this.httpClient.post(this.baseURL + `/verify`, purchaseInvoiceIds);
  // }
}
