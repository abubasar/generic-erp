import { Injectable } from "@angular/core";
import { VoucherEntryApiService } from "app/shared/api/account/voucher-entry-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { AddUpdateResponseDTO } from "app/views/purchase/models/add-update-response-dto.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { Observable, map } from "rxjs";
import { VoucherEntryRequestDTO } from "../models/voucher-entry/voucher-entry-request-dto.model";
import { VoucherEntryResponseDTO } from "../models/voucher-entry/voucher-entry-response-dto.model";
import { VoucherEntrySearchRequestDTO } from "../models/voucher-entry/voucher-entry-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class VoucherEntryService {
  constructor(private api: VoucherEntryApiService) {}

  getVoucherEntries(
    voucherEntryRequest: VoucherEntrySearchRequestDTO
  ): Observable<SearchResponse<VoucherEntryResponseDTO>> {
    return this.api
      .getAll(voucherEntryRequest)
      .pipe(
        map((response: SearchResponse<VoucherEntryResponseDTO>) => response)
      );
  }

  getVoucherEntryById(
    id: string
  ): Observable<GeneralResponse<VoucherEntryResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<VoucherEntryResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createVoucherEntry(
    voucherEntryDTO: VoucherEntryRequestDTO
  ): Observable<GeneralResponse<AddUpdateResponseDTO>> {
    return this.api
      .create(voucherEntryDTO)
      .pipe(map((response: GeneralResponse<AddUpdateResponseDTO>) => response));
  }

  updateVoucherEntry(
    voucherEntryDTO: VoucherEntryRequestDTO
  ): Observable<GeneralResponse<AddUpdateResponseDTO>> {
    return this.api
      .update(voucherEntryDTO)
      .pipe(map((response: GeneralResponse<AddUpdateResponseDTO>) => response));
  }

  deleteVoucherEntry(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkVoucherEntry(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveVoucherEntry(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostVoucherEntry(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
