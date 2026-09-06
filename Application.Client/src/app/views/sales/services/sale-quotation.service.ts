import { Injectable } from "@angular/core";
import { SaleQuotationApiService } from "app/shared/api/sale/sale-quotation-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { AddUpdateResponseDTO } from "app/views/purchase/models/add-update-response-dto.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { map, Observable } from "rxjs";
import { SaleQuotationAggregatorModel } from "../models/sale-quotation/sale-quotation-aggregator.model";
import { SaleQuotationRequestDTO } from "../models/sale-quotation/sale-quotation-request-dto.model";
import { SaleQuotationResponseDTO } from "../models/sale-quotation/sale-quotation-response-dto.model";
import { SaleQuotationSearchRequestDTO } from "../models/sale-quotation/sale-quotation-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class SaleQuotationService {
  constructor(private api: SaleQuotationApiService) {}

  getSaleQuotations(
    saleQuotationRequest: SaleQuotationSearchRequestDTO
  ): Observable<SearchResponse<SaleQuotationResponseDTO>> {
    return this.api
      .getAll(saleQuotationRequest)
      .pipe(
        map((response: SearchResponse<SaleQuotationResponseDTO>) => response)
      );
  }

  reportAggregates(
    saleQuotationRequest: SaleQuotationSearchRequestDTO
  ): Observable<GeneralResponse<SaleQuotationAggregatorModel>> {
    return this.api
      .reportAggregates(saleQuotationRequest)
      .pipe(
        map(
          (response: GeneralResponse<SaleQuotationAggregatorModel>) => response
        )
      );
  }

  getSaleQuotationById(
    id: string
  ): Observable<GeneralResponse<SaleQuotationResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<SaleQuotationResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createSaleQuotation(
    saleQuotationDTO: SaleQuotationRequestDTO
  ): Observable<GeneralResponse<AddUpdateResponseDTO>> {
    return this.api
      .create(saleQuotationDTO)
      .pipe(map((response: GeneralResponse<AddUpdateResponseDTO>) => response));
  }

  updateSaleQuotation(
    saleQuotationDTO: SaleQuotationRequestDTO
  ): Observable<GeneralResponse<AddUpdateResponseDTO>> {
    return this.api
      .update(saleQuotationDTO)
      .pipe(map((response: GeneralResponse<AddUpdateResponseDTO>) => response));
  }

  deleteSaleQuotation(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkSaleQuotation(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveSaleQuotation(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostSaleQuotation(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
