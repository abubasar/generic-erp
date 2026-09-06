import { Injectable } from "@angular/core";
import { SaleReturnApiService } from "app/shared/api/sale/sale-return-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { Observable, map } from "rxjs";
import { SaleReturnAggregatorModel } from "../models/sale-return/sale-return-aggregator.model";
import { SaleReturnRequestDTO } from "../models/sale-return/sale-return-request-dto.model";
import { SaleReturnResponseDTO } from "../models/sale-return/sale-return-response-dto.model";
import { SaleReturnSearchRequestDTO } from "../models/sale-return/sale-return-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class SaleReturnService {
  constructor(private api: SaleReturnApiService) {}

  getSaleReturns(
    saleReturnRequest: SaleReturnSearchRequestDTO
  ): Observable<SearchResponse<SaleReturnResponseDTO>> {
    return this.api
      .getAll(saleReturnRequest)
      .pipe(map((response: SearchResponse<SaleReturnResponseDTO>) => response));
  }

  reportAggregates(
    saleReturnRequest: SaleReturnSearchRequestDTO
  ): Observable<GeneralResponse<SaleReturnAggregatorModel>> {
    return this.api
      .reportAggregates(saleReturnRequest)
      .pipe(
        map((response: GeneralResponse<SaleReturnAggregatorModel>) => response)
      );
  }

  getSaleReturnById(
    id: string
  ): Observable<GeneralResponse<SaleReturnResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<SaleReturnResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createSaleReturn(
    saleReturnDTO: SaleReturnRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(saleReturnDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateSaleReturn(
    saleReturnDTO: SaleReturnRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(saleReturnDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteSaleReturn(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkSaleReturn(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveSaleReturn(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostSaleReturn(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
