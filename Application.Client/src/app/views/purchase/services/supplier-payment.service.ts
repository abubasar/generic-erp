import { Injectable } from "@angular/core";
import { SupplierPaymentApiService } from "app/shared/api/purchase/supplier-payment-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { Observable, map } from "rxjs";
import { PendingCheckedCount } from "../models/pending-checked-count";
import { SupplierPaymentAggregatorModel } from "../models/supplier-payment/supplier-payment-aggregator.model";
import { SupplierPaymentRequestDTO } from "../models/supplier-payment/supplier-payment-request-dto.model";
import { SupplierPaymentResponseDTO } from "../models/supplier-payment/supplier-payment-response-dto.model";
import { SupplierPaymentSearchRequestDTO } from "../models/supplier-payment/supplier-payment-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class SupplierPaymentService {
  constructor(private api: SupplierPaymentApiService) {}

  getSupplierPayments(
    supplierPaymentRequest: SupplierPaymentSearchRequestDTO
  ): Observable<SearchResponse<SupplierPaymentResponseDTO>> {
    return this.api
      .getAll(supplierPaymentRequest)
      .pipe(
        map((response: SearchResponse<SupplierPaymentResponseDTO>) => response)
      );
  }

  reportAggregates(
    supplierPaymentRequest: SupplierPaymentSearchRequestDTO
  ): Observable<GeneralResponse<SupplierPaymentAggregatorModel>> {
    return this.api
      .reportAggregates(supplierPaymentRequest)
      .pipe(
        map(
          (response: GeneralResponse<SupplierPaymentAggregatorModel>) =>
            response
        )
      );
  }

  getSupplierPaymentById(
    id: string
  ): Observable<GeneralResponse<SupplierPaymentResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<SupplierPaymentResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createSupplierPayment(
    supplierPaymentDTO: SupplierPaymentRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .create(supplierPaymentDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  updateSupplierPayment(
    supplierPaymentDTO: SupplierPaymentRequestDTO
  ): Observable<GeneralResponse<string>> {
    return this.api
      .update(supplierPaymentDTO)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  deleteSupplierPayment(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkSupplierPayment(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveSupplierPayment(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostSupplierPayment(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }
}
