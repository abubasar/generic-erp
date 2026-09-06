import { Injectable } from "@angular/core";
import { FundTransferTransactionTypeApiService } from "app/shared/api/configuration/fund-transfer-transaction-type-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { Observable, map } from "rxjs";
import { FundTransferTransactionTypeRequest } from "../models/fund-transfer-transaction-type/fund-transfer-transaction-type-request.model";
import { FundTransferTransactionType } from "../models/fund-transfer-transaction-type/fund-transfer-transaction-type.model";

@Injectable({
  providedIn: "root",
})
export class FundTransferTransactionTypeService {
  constructor(private api: FundTransferTransactionTypeApiService) {}

  getAllFundTransferTransactionTypes(): Observable<
    SearchResponse<FundTransferTransactionType>
  > {
    return this.api
      .getAlls()
      .pipe(
        map((response: SearchResponse<FundTransferTransactionType>) => response)
      );
  }
  getFundTransferTransactionTypes(
    fundTransferTransactionTypeRequest: FundTransferTransactionTypeRequest
  ): Observable<SearchResponse<FundTransferTransactionType>> {
    return this.api
      .getAll(fundTransferTransactionTypeRequest)
      .pipe(
        map((response: SearchResponse<FundTransferTransactionType>) => response)
      );
  }

  createFundTransferTransactionType(
    fundTransferTransactionType: FundTransferTransactionType
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(fundTransferTransactionType)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateFundTransferTransactionType(
    fundTransferTransactionType: FundTransferTransactionType
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(fundTransferTransactionType)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteFundTransferTransactionType(
    id: string
  ): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }
}
