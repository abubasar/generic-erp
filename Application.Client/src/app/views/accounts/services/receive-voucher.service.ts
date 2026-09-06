import { Injectable } from "@angular/core";
import { ReceiveVoucherApiService } from "app/shared/api/account/receive-voucher-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { Observable, map } from "rxjs";
import { ReceiveVoucherAggregatorModel } from "../models/receive-voucher/receive-voucher-aggregator.model";
import { ReceiveVoucherRequestDTO } from "../models/receive-voucher/receive-voucher-request-dto.model";
import { ReceiveVoucherResponseDTO } from "../models/receive-voucher/receive-voucher-response-dto.model";
import { ReceiveVoucherSearchRequestDTO } from "../models/receive-voucher/receive-voucher-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class ReceiveVoucherService {
  constructor(private api: ReceiveVoucherApiService) {}

  getReceiveVouchers(
    receiveVoucherRequest: ReceiveVoucherSearchRequestDTO
  ): Observable<SearchResponse<ReceiveVoucherResponseDTO>> {
    return this.api
      .getAll(receiveVoucherRequest)
      .pipe(
        map((response: SearchResponse<ReceiveVoucherResponseDTO>) => response)
      );
  }

  reportAggregates(
    receiveVoucherRequest: ReceiveVoucherSearchRequestDTO
  ): Observable<GeneralResponse<ReceiveVoucherAggregatorModel>> {
    return this.api
      .reportAggregates(receiveVoucherRequest)
      .pipe(
        map(
          (response: GeneralResponse<ReceiveVoucherAggregatorModel>) => response
        )
      );
  }

  getReceiveVoucherById(
    id: string
  ): Observable<GeneralResponse<ReceiveVoucherResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<ReceiveVoucherResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createReceiveVoucher(
    receiveVoucherDTO: ReceiveVoucherRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(receiveVoucherDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateReceiveVoucher(
    receiveVoucherDTO: ReceiveVoucherRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(receiveVoucherDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteReceiveVoucher(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkReceiveVoucher(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveReceiveVoucher(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostReceiveVoucher(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
