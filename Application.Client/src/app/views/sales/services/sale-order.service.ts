import { Injectable } from "@angular/core";
import { SaleOrderApiService } from "app/shared/api/sale/sale-order-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { Observable, map } from "rxjs";
import { SaleOrderAggregatorModel } from "../models/sale-order/sale-order-aggregator.model";
import { SaleOrderRequestDTO } from "../models/sale-order/sale-order-request-dto.model";
import { SaleOrderResponseDTO } from "../models/sale-order/sale-order-response-dto.model";
import { SaleOrderSearchRequestDTO } from "../models/sale-order/sale-order-search-request-dto.model";
import { HttpHeaders } from "@angular/common/http";

@Injectable({
  providedIn: "root",
})
export class SaleOrderService {
  constructor(private api: SaleOrderApiService) {}

  getSaleOrders(
    saleOrderRequest: SaleOrderSearchRequestDTO
  ): Observable<SearchResponse<SaleOrderResponseDTO>> {
    return this.api
      .getAll(saleOrderRequest)
      .pipe(map((response: SearchResponse<SaleOrderResponseDTO>) => response));
  }

  reportAggregates(
    saleOrderRequest: SaleOrderSearchRequestDTO
  ): Observable<GeneralResponse<SaleOrderAggregatorModel>> {
    return this.api
      .reportAggregates(saleOrderRequest)
      .pipe(
        map((response: GeneralResponse<SaleOrderAggregatorModel>) => response)
      );
  }

  getSaleOrderById(
    id: string
  ): Observable<GeneralResponse<SaleOrderResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(map((response: GeneralResponse<SaleOrderResponseDTO>) => response));
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createSaleOrder(
    saleOrderDTO: SaleOrderRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(saleOrderDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateSaleOrder(
    saleOrderDTO: SaleOrderRequestDTO,
    headers: HttpHeaders
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(saleOrderDTO, headers)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteSaleOrder(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkSaleOrder(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveSaleOrder(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  closeSaleOrder(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .close(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostSaleOrder(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
