import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { PurchaseOrderRequestDTO } from "app/views/purchase/models/purchase-order/purchase-order-request-dto.model";
import { PurchaseOrderSearchRequestDTO } from "app/views/purchase/models/purchase-order/purchase-order-search-request-dto.model";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class PurchaseOrderApiService {
  baseURL = environment.apiURL + "/purchaseOrder";

  constructor(private httpClient: HttpClient) {}

  getAlls() {
    return this.httpClient.post(this.baseURL + "/search", { page: -1 });
  }

  getAll(request: PurchaseOrderSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  reportAggregates(request: PurchaseOrderSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/report-aggregates", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }

  create(purchaseOrder: PurchaseOrderRequestDTO) {
    return this.httpClient.post(this.baseURL, purchaseOrder);
  }

  update(purchaseOrder: PurchaseOrderRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", purchaseOrder);
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

  readyForGrn(id: string) {
    return this.httpClient.post(this.baseURL + `/ready-for-grn/` + id, {});
  }

  unpost(id: string, status: number) {
    return this.httpClient.post(this.baseURL + `/unpost/${id}/${status}`, {});
  }

  send(id: string) {
    return this.httpClient.post(this.baseURL + `/send/` + id, {});
  }

  close(id: string) {
    return this.httpClient.post(this.baseURL + `/close/${id}`, {});
  }

  // verify(purchaseOrderIds: string[]) {
  //   return this.httpClient.post(this.baseURL + `/verify`, purchaseOrderIds);
  // }

  getSupplierTransactionsAgainstPO(purchaseOrderId: string) {
    return this.httpClient.get(
      this.baseURL + `/supplier-transactions-against-po/${purchaseOrderId}`
    );
  }
  lastPoDetails(supplierId: string, productId: string) {
    return this.httpClient.post(
      this.baseURL + `/last-po-details/${supplierId}/${productId}`,
      {}
    );
  }
}
