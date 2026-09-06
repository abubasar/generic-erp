import { Injectable } from "@angular/core";
import { PurchaseOrderApiService } from "app/shared/api/purchase/purchase-order-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { map, Observable } from "rxjs";
import { PendingCheckedCount } from "../models/pending-checked-count";
import { PurchaseOrderAggregatorModel } from "../models/purchase-order/purchase-order-aggregator.model";
import { PurchaseOrderRequestDTO } from "../models/purchase-order/purchase-order-request-dto.model";
import {
  LastPoDetails,
  PurchaseOrderResponseDTO,
} from "../models/purchase-order/purchase-order-response-dto.model";
import { PurchaseOrderSearchRequestDTO } from "../models/purchase-order/purchase-order-search-request-dto.model";
import { SupplierTransactionsAgainstPOResponseDTO } from "../models/purchase-order/supplier-transactions-against-po-response-dto.model";

@Injectable({
  providedIn: "root",
})
export class PurchaseOrderService {
  constructor(private api: PurchaseOrderApiService) {}

  getAllPurchaseOrders(): Observable<SearchResponse<PurchaseOrderResponseDTO>> {
    return this.api
      .getAlls()
      .pipe(
        map((response: SearchResponse<PurchaseOrderResponseDTO>) => response)
      );
  }

  reportAggregates(
    purchaseOrderRequest: PurchaseOrderSearchRequestDTO
  ): Observable<GeneralResponse<PurchaseOrderAggregatorModel>> {
    return this.api
      .reportAggregates(purchaseOrderRequest)
      .pipe(
        map(
          (response: GeneralResponse<PurchaseOrderAggregatorModel>) => response
        )
      );
  }

  getPurchaseOrderById(
    id: string
  ): Observable<GeneralResponse<PurchaseOrderResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<PurchaseOrderResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }
  getPurchaseOrders(
    purchaseOrderRequest: PurchaseOrderSearchRequestDTO
  ): Observable<SearchResponse<PurchaseOrderResponseDTO>> {
    return this.api
      .getAll(purchaseOrderRequest)
      .pipe(
        map((response: SearchResponse<PurchaseOrderResponseDTO>) => response)
      );
  }

  createPurchaseOrder(
    purchaseOrderDTO: PurchaseOrderRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(purchaseOrderDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updatePurchaseOrder(
    purchaseOrderDTO: PurchaseOrderRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(purchaseOrderDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deletePurchaseOrder(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkPurchaseOrder(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approvePurchaseOrder(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  readyForGrnPurchaseOrder(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .readyForGrn(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostPurchaseOrder(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  sendPurchaseOrder(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .send(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  closePurchaseOrder(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .close(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  getSupplierTransactionsAgainstPO(
    purchaseOrderId: string
  ): Observable<GeneralResponse<SupplierTransactionsAgainstPOResponseDTO[]>> {
    return this.api
      .getSupplierTransactionsAgainstPO(purchaseOrderId)
      .pipe(
        map(
          (
            response: GeneralResponse<
              SupplierTransactionsAgainstPOResponseDTO[]
            >
          ) => response
        )
      );
  }

  getLastPoDetails(
    supplierId: string,
    productId: string
  ): Observable<GeneralResponse<LastPoDetails>> {
    return this.api
      .lastPoDetails(supplierId, productId)
      .pipe(map((response: GeneralResponse<LastPoDetails>) => response));
  }
}
