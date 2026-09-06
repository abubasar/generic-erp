import { Injectable } from "@angular/core";
import { FundTransferApiService } from "app/shared/api/account/fund-transfer-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { AddUpdateResponseDTO } from "app/views/purchase/models/add-update-response-dto.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { Observable, map } from "rxjs";
import { FundTransferAggregatorModel } from "../models/fund-transfer/fund-transfer-aggregator.model";
import { FundTransferRequestDTO } from "../models/fund-transfer/fund-transfer-request-dto.model";
import { FundTransferResponseDTO } from "../models/fund-transfer/fund-transfer-response-dto.model";
import { FundTransferSearchRequestDTO } from "../models/fund-transfer/fund-transfer-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class FundTransferService {
  constructor(private api: FundTransferApiService) {}

  getFundTransfers(
    fundTransferRequest: FundTransferSearchRequestDTO
  ): Observable<SearchResponse<FundTransferResponseDTO>> {
    return this.api
      .getAll(fundTransferRequest)
      .pipe(
        map((response: SearchResponse<FundTransferResponseDTO>) => response)
      );
  }

  reportAggregates(
    fundTransferRequest: FundTransferSearchRequestDTO
  ): Observable<GeneralResponse<FundTransferAggregatorModel>> {
    return this.api
      .reportAggregates(fundTransferRequest)
      .pipe(
        map(
          (response: GeneralResponse<FundTransferAggregatorModel>) => response
        )
      );
  }

  getFundTransferById(
    id: string
  ): Observable<GeneralResponse<FundTransferResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<FundTransferResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createFundTransfer(
    fundTransferDTO: FundTransferRequestDTO
  ): Observable<GeneralResponse<AddUpdateResponseDTO>> {
    return this.api
      .create(fundTransferDTO)
      .pipe(map((response: GeneralResponse<AddUpdateResponseDTO>) => response));
  }

  updateFundTransfer(
    fundTransferDTO: FundTransferRequestDTO
  ): Observable<GeneralResponse<AddUpdateResponseDTO>> {
    return this.api
      .update(fundTransferDTO)
      .pipe(map((response: GeneralResponse<AddUpdateResponseDTO>) => response));
  }

  deleteFundTransfer(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkFundTransfer(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveFundTransfer(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostFundTransfer(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
