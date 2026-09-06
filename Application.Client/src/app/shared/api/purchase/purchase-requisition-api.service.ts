import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { PurchaseRequisitionRequestDTO } from "app/views/purchase/models/purchase-requisition/purchase-requisition-request-dto.model";
import { PurchaseRequisitionSearchRequestDTO } from "app/views/purchase/models/purchase-requisition/purchase-requisition-search-request-dto.model";
import { PurchaseRequisitionResponseDTO } from "app/views/purchase/models/purchase-requisition/purchase-requisition-response-dto.model";
import { SendRFQRequestDTO } from "app/views/purchase/models/purchase-requisition/send-rfq-request-dto.model";

import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class PurchaseRequisitionApiService {
  baseURL = environment.apiURL + "/purchaseRequisition";

  constructor(private httpClient: HttpClient) {}

  getAll(request: PurchaseRequisitionSearchRequestDTO) {
    return this.httpClient.post(this.baseURL + "/search", request);
  }

  getById(id: string) {
    return this.httpClient.get(this.baseURL + `/${id}`);
  }

  getPendingCheckedCount() {
    return this.httpClient.get(this.baseURL + "/pending-checked");
  }
  create(purchaseRequisition: PurchaseRequisitionRequestDTO) {
    return this.httpClient.post(this.baseURL, purchaseRequisition);
  }

  update(purchaseRequisition: PurchaseRequisitionRequestDTO) {
    return this.httpClient.post(this.baseURL + "/update", purchaseRequisition);
  }
  preparRFQ(purchaseRequisition: PurchaseRequisitionRequestDTO) {
    return this.httpClient.post(
      this.baseURL + "/prepare-rfq",
      purchaseRequisition
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

  sendRFQ(sendRFQRequest: SendRFQRequestDTO) {
    {
      return this.httpClient.post(this.baseURL + "/send-rfq", sendRFQRequest);
    }
  }
  getRfqSentSuppliers(requisitionId: string) {
    return this.httpClient.get(
      this.baseURL + `/rfq-sent-suppliers/${requisitionId}`
    );
  }
}
