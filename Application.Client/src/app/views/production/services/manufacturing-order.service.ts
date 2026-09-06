import { Injectable } from "@angular/core";
import { ManufacturingOrderApiService } from "app/shared/api/production/manufacturing-order-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { map, Observable } from "rxjs";
import { ManufacturingOrderAggregatorModel } from "../models/manufacturing-order/manufacturing-order-aggregator.model";
import { ManufacturingOrderRequestDTO } from "../models/manufacturing-order/manufacturing-order-request-dto.model";
import { ManufacturingOrderResponseDTO } from "../models/manufacturing-order/manufacturing-order-response-dto.model";
import { ManufacturingOrderSearchRequestDTO } from "../models/manufacturing-order/manufacturing-order-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class ManufacturingOrderService {
  constructor(private api: ManufacturingOrderApiService) {}

  getManufacturingOrders(
    manufacturingOrderRequest: ManufacturingOrderSearchRequestDTO
  ): Observable<SearchResponse<ManufacturingOrderResponseDTO>> {
    return this.api
      .getAll(manufacturingOrderRequest)
      .pipe(
        map(
          (response: SearchResponse<ManufacturingOrderResponseDTO>) => response
        )
      );
  }

  reportAggregates(
    manufacturingOrderRequest: ManufacturingOrderSearchRequestDTO
  ): Observable<GeneralResponse<ManufacturingOrderAggregatorModel>> {
    return this.api
      .reportAggregates(manufacturingOrderRequest)
      .pipe(
        map(
          (response: GeneralResponse<ManufacturingOrderAggregatorModel>) =>
            response
        )
      );
  }

  getManufacturingOrderById(
    id: string
  ): Observable<GeneralResponse<ManufacturingOrderResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map(
          (response: GeneralResponse<ManufacturingOrderResponseDTO>) => response
        )
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createManufacturingOrder(
    manufacturingOrderDTO: ManufacturingOrderRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(manufacturingOrderDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateManufacturingOrder(
    manufacturingOrderDTO: ManufacturingOrderRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(manufacturingOrderDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteManufacturingOrder(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkManufacturingOrder(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveManufacturingOrder(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostManufacturingOrder(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
