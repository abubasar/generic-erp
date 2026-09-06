import { Injectable } from "@angular/core";
import { PurchaseInvoiceApiService } from "app/shared/api/purchase/purchase-invoice-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { Observable, map } from "rxjs";
import { PendingCheckedCount } from "../models/pending-checked-count";
import { PurchaseInvoiceAggregatorModel } from "../models/purchase-invoice/purchase-invoice-aggregator.model";
import { PurchaseInvoiceRequestDTO } from "../models/purchase-invoice/purchase-invoice-request-dto.model";
import { PurchaseInvoiceResponseDTO } from "../models/purchase-invoice/purchase-invoice-response-dto.model";
import { PurchaseInvoiceSearchRequestDTO } from "../models/purchase-invoice/purchase-invoice-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class PurchaseInvoiceService {
  constructor(private api: PurchaseInvoiceApiService) {}

  getPurchaseInvoices(
    purchaseInvoiceRequest: PurchaseInvoiceSearchRequestDTO
  ): Observable<SearchResponse<PurchaseInvoiceResponseDTO>> {
    return this.api
      .getAll(purchaseInvoiceRequest)
      .pipe(
        map((response: SearchResponse<PurchaseInvoiceResponseDTO>) => response)
      );
  }

  reportAggregates(
    purchaseInvoiceRequest: PurchaseInvoiceSearchRequestDTO
  ): Observable<GeneralResponse<PurchaseInvoiceAggregatorModel>> {
    return this.api
      .reportAggregates(purchaseInvoiceRequest)
      .pipe(
        map(
          (response: GeneralResponse<PurchaseInvoiceAggregatorModel>) =>
            response
        )
      );
  }

  getPurchaseInvoiceById(
    id: string
  ): Observable<GeneralResponse<PurchaseInvoiceResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<PurchaseInvoiceResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }
  createPurchaseInvoice(
    purchaseInvoiceDTO: PurchaseInvoiceRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(purchaseInvoiceDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updatePurchaseInvoice(
    purchaseInvoiceDTO: PurchaseInvoiceRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(purchaseInvoiceDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deletePurchaseInvoice(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkPurchaseInvoice(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approvePurchaseInvoice(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostPurchaseInvoice(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  resetSupplierPaymentCodeAndAdvanceAmount(
    id: string
  ): Observable<GeneralResponse<string>> {
    return this.api
      .resetAdvanceAmount(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
