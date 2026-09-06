import { Injectable } from "@angular/core";
import { ProductionApiService } from "app/shared/api/production/production-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { map, Observable } from "rxjs";
import { ProductionAggregatorModel } from "../models/production/production-aggregator.model";
import { ProductionRequestDTO } from "../models/production/production-request-dto.model";
import { ProductionResponseDTO } from "../models/production/production-response-dto.model";
import { ProductionSearchRequestDTO } from "../models/production/production-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class ProductionService {
  constructor(private api: ProductionApiService) {}

  getProductions(
    productionRequest: ProductionSearchRequestDTO
  ): Observable<SearchResponse<ProductionResponseDTO>> {
    return this.api
      .getAll(productionRequest)
      .pipe(map((response: SearchResponse<ProductionResponseDTO>) => response));
  }

  reportAggregates(
    productionRequest: ProductionSearchRequestDTO
  ): Observable<GeneralResponse<ProductionAggregatorModel>> {
    return this.api
      .reportAggregates(productionRequest)
      .pipe(
        map((response: GeneralResponse<ProductionAggregatorModel>) => response)
      );
  }

  getProductionById(
    id: string
  ): Observable<GeneralResponse<ProductionResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<ProductionResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createProduction(
    productionDTO: ProductionRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(productionDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateProduction(
    productionDTO: ProductionRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(productionDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteProduction(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkProduction(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveProduction(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostProduction(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
