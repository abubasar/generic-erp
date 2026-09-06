import { Injectable } from "@angular/core";
import { PaymentVoucherApiService } from "app/shared/api/account/payment-voucher-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { Observable, map } from "rxjs";
import { PaymentVoucherAggregatorModel } from "../models/payment-voucher/payment-voucher-aggregator.model";
import { PaymentVoucherRequestDTO } from "../models/payment-voucher/payment-voucher-request-dto.model";
import { PaymentVoucherResponseDTO } from "../models/payment-voucher/payment-voucher-response-dto.model";
import { PaymentVoucherSearchRequestDTO } from "../models/payment-voucher/payment-voucher-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class PaymentVoucherService {
  constructor(private api: PaymentVoucherApiService) {}

  getPaymentVouchers(
    paymentVoucherRequest: PaymentVoucherSearchRequestDTO
  ): Observable<SearchResponse<PaymentVoucherResponseDTO>> {
    return this.api
      .getAll(paymentVoucherRequest)
      .pipe(
        map((response: SearchResponse<PaymentVoucherResponseDTO>) => response)
      );
  }

  reportAggregates(
    paymentVoucherRequest: PaymentVoucherSearchRequestDTO
  ): Observable<GeneralResponse<PaymentVoucherAggregatorModel>> {
    return this.api
      .reportAggregates(paymentVoucherRequest)
      .pipe(
        map(
          (response: GeneralResponse<PaymentVoucherAggregatorModel>) => response
        )
      );
  }

  getPaymentVoucherById(
    id: string
  ): Observable<GeneralResponse<PaymentVoucherResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<PaymentVoucherResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createPaymentVoucher(
    paymentVoucherDTO: PaymentVoucherRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(paymentVoucherDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updatePaymentVoucher(
    paymentVoucherDTO: PaymentVoucherRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(paymentVoucherDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deletePaymentVoucher(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkPaymentVoucher(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approvePaymentVoucher(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostPaymentVoucher(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
