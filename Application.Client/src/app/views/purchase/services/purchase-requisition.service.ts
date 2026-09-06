import { Injectable } from "@angular/core";
import { PurchaseRequisitionApiService } from "app/shared/api/purchase/purchase-requisition-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { PendingCheckedCount } from "../models/pending-checked-count";
import { PurchaseRequisitionRequestDTO } from "../models/purchase-requisition/purchase-requisition-request-dto.model";
import { PurchaseRequisitionResponseDTO } from "../models/purchase-requisition/purchase-requisition-response-dto.model";
import { PurchaseRequisitionSearchRequestDTO } from "../models/purchase-requisition/purchase-requisition-search-request-dto.model";
import { RFQSentSupplierResponseDTO } from "../models/purchase-requisition/rfq-sent-supplier-response-dto.model";
import { SendRFQRequestDTO } from "../models/purchase-requisition/send-rfq-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class PurchaseRequisitionService {
  constructor(private api: PurchaseRequisitionApiService) {}

  getPurchaseRequisitions(
    purchaseRequisitionRequest: PurchaseRequisitionSearchRequestDTO
  ): Observable<SearchResponse<PurchaseRequisitionResponseDTO>> {
    return this.api
      .getAll(purchaseRequisitionRequest)
      .pipe(
        map(
          (response: SearchResponse<PurchaseRequisitionResponseDTO>) => response
        )
      );
  }

  getPurchaseRequisitionById(
    id: string
  ): Observable<GeneralResponse<PurchaseRequisitionResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map(
          (response: GeneralResponse<PurchaseRequisitionResponseDTO>) =>
            response
        )
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }
  createPurchaseRequisition(
    purchaseRequisitionDTO: PurchaseRequisitionRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(purchaseRequisitionDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updatePurchaseRequisition(
    purchaseRequisitionDTO: PurchaseRequisitionRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(purchaseRequisitionDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  preparePurchaseRequisitionForRFQ(
    purchaseRequisitionDTO: PurchaseRequisitionRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .preparRFQ(purchaseRequisitionDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deletePurchaseRequisition(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkPurchaseRequisition(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approvePurchaseRequisition(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostPurchaseRequisition(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  sendRFQToVendor(
    request: SendRFQRequestDTO
  ): Observable<GeneralResponse<number>> {
    return this.api
      .sendRFQ(request)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  getRfqSentSuppliers(
    requisitionId: string
  ): Observable<GeneralResponse<RFQSentSupplierResponseDTO[]>> {
    return this.api
      .getRfqSentSuppliers(requisitionId)
      .pipe(
        map(
          (response: GeneralResponse<RFQSentSupplierResponseDTO[]>) => response
        )
      );
  }
}
