import { Injectable } from "@angular/core";
import { ReceivePaymentApiService } from "app/shared/api/sale/receive-payment-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { Observable, map } from "rxjs";
import { ReceivePaymentAggregatorModel } from "../models/receive-payment/receive-payment-aggregator.model";
import { ReceivePaymentRequestDTO } from "../models/receive-payment/receive-payment-request-dto.model";
import { ReceivePaymentResponseDTO } from "../models/receive-payment/receive-payment-response-dto.model";
import { ReceivePaymentSearchRequestDTO } from "../models/receive-payment/receive-payment-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class ReceivePaymentService {
  constructor(private api: ReceivePaymentApiService) {}

  getReceivePayments(
    receivePaymentRequest: ReceivePaymentSearchRequestDTO
  ): Observable<SearchResponse<ReceivePaymentResponseDTO>> {
    return this.api
      .getAll(receivePaymentRequest)
      .pipe(
        map((response: SearchResponse<ReceivePaymentResponseDTO>) => response)
      );
  }

  reportAggregates(
    receivePaymentRequest: ReceivePaymentSearchRequestDTO
  ): Observable<GeneralResponse<ReceivePaymentAggregatorModel>> {
    return this.api
      .reportAggregates(receivePaymentRequest)
      .pipe(
        map(
          (response: GeneralResponse<ReceivePaymentAggregatorModel>) => response
        )
      );
  }

  getReceivePaymentById(
    id: string
  ): Observable<GeneralResponse<ReceivePaymentResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<ReceivePaymentResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createReceivePayment(
    receivePaymentDTO: ReceivePaymentRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(receivePaymentDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateReceivePayment(
    receivePaymentDTO: ReceivePaymentRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(receivePaymentDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteReceivePayment(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkReceivePayment(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveReceivePayment(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  sendMoneyReceipt(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .sendToCustomer(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostReceivePayment(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
