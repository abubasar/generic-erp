import { Injectable } from "@angular/core";
import { ReceivePaymentAgainstSaleApiService } from "app/shared/api/sale/receive-payment-against-sale-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { AddUpdateResponseDTO } from "app/views/purchase/models/add-update-response-dto.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { Observable, map } from "rxjs";
import { ReceivePaymentAgainstSaleAggregatorModel } from "../models/receive-payment-against-sale/receive-payment-against-sale-aggregator.model";
import { ReceivePaymentAgainstSaleRequestDTO } from "../models/receive-payment-against-sale/receive-payment-against-sale-request-dto.model";
import { ReceivePaymentAgainstSaleResponseDTO } from "../models/receive-payment-against-sale/receive-payment-against-sale-response-dto.model";
import { ReceivePaymentAgainstSaleSearchRequestDTO } from "../models/receive-payment-against-sale/receive-payment-against-sale-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class ReceivePaymentAgainstSaleService {
  constructor(private api: ReceivePaymentAgainstSaleApiService) {}

  getReceivePaymentAgainstSales(
    receivePaymentAgainstSaleRequest: ReceivePaymentAgainstSaleSearchRequestDTO
  ): Observable<SearchResponse<ReceivePaymentAgainstSaleResponseDTO>> {
    return this.api
      .getAll(receivePaymentAgainstSaleRequest)
      .pipe(
        map(
          (response: SearchResponse<ReceivePaymentAgainstSaleResponseDTO>) =>
            response
        )
      );
  }

  reportAggregates(
    receivePaymentAgainstSaleRequest: ReceivePaymentAgainstSaleSearchRequestDTO
  ): Observable<GeneralResponse<ReceivePaymentAgainstSaleAggregatorModel>> {
    return this.api
      .reportAggregates(receivePaymentAgainstSaleRequest)
      .pipe(
        map(
          (
            response: GeneralResponse<ReceivePaymentAgainstSaleAggregatorModel>
          ) => response
        )
      );
  }

  getReceivePaymentAgainstSaleById(
    id: string
  ): Observable<GeneralResponse<ReceivePaymentAgainstSaleResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map(
          (response: GeneralResponse<ReceivePaymentAgainstSaleResponseDTO>) =>
            response
        )
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createReceivePaymentAgainstSale(
    receivePaymentAgainstSaleDTO: ReceivePaymentAgainstSaleRequestDTO
  ): Observable<GeneralResponse<AddUpdateResponseDTO>> {
    return this.api
      .create(receivePaymentAgainstSaleDTO)
      .pipe(map((response: GeneralResponse<AddUpdateResponseDTO>) => response));
  }

  updateReceivePaymentAgainstSale(
    receivePaymentAgainstSaleDTO: ReceivePaymentAgainstSaleRequestDTO
  ): Observable<GeneralResponse<AddUpdateResponseDTO>> {
    return this.api
      .update(receivePaymentAgainstSaleDTO)
      .pipe(map((response: GeneralResponse<AddUpdateResponseDTO>) => response));
  }

  deleteReceivePaymentAgainstSale(
    id: string
  ): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkReceivePaymentAgainstSale(
    id: string
  ): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveReceivePaymentAgainstSale(
    id: string
  ): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostReceivePaymentAgainstSale(
    id: string,
    status:number
  ): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
