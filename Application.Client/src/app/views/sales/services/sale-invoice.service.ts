import { Injectable } from "@angular/core";
import { SaleInvoiceApiService } from "app/shared/api/sale/sale-invoice-api.service";
import { GeneralResponse } from "app/shared/models/wrappers/generalResponse.model";
import { SearchResponse } from "app/shared/models/wrappers/searchResponse.model";
import { AddUpdateResponseDTO } from "app/views/purchase/models/add-update-response-dto.model";
import { PendingCheckedCount } from "app/views/purchase/models/pending-checked-count";
import { CustomerDiscountReportResponseDTO } from "app/views/report/models/customer-monthly-discount-report/customer-discount-report-response-dto";
import { CustomerDiscountReportSearchRequestDTO } from "app/views/report/models/customer-monthly-discount-report/customer-discount-report-search-request-dto";
import { Observable, map } from "rxjs";
import { SaleInvoiceAggregatorModel } from "../models/sale-invoice/sale-invoice-aggregator.model";
import { SaleInvoiceRequestDTO } from "../models/sale-invoice/sale-invoice-request-dto.model";
import { SaleInvoiceResponseDTO } from "../models/sale-invoice/sale-invoice-response-dto.model";
import { SaleInvoiceSearchRequestDTO } from "../models/sale-invoice/sale-invoice-search-request-dto.model";

@Injectable({
  providedIn: "root",
})
export class SaleInvoiceService {
  constructor(private api: SaleInvoiceApiService) {}

  getSaleInvoices(
    saleInvoiceRequest: SaleInvoiceSearchRequestDTO
  ): Observable<SearchResponse<SaleInvoiceResponseDTO>> {
    return this.api
      .getAll(saleInvoiceRequest)
      .pipe(
        map((response: SearchResponse<SaleInvoiceResponseDTO>) => response)
      );
  }

  reportAggregates(
    saleInvoiceRequest: SaleInvoiceSearchRequestDTO
  ): Observable<GeneralResponse<SaleInvoiceAggregatorModel>> {
    return this.api
      .reportAggregates(saleInvoiceRequest)
      .pipe(
        map((response: GeneralResponse<SaleInvoiceAggregatorModel>) => response)
      );
  }

  getSaleInvoiceById(
    id: string
  ): Observable<GeneralResponse<SaleInvoiceResponseDTO>> {
    return this.api
      .getById(id)
      .pipe(
        map((response: GeneralResponse<SaleInvoiceResponseDTO>) => response)
      );
  }

  getPendingCheckedCount(): Observable<GeneralResponse<PendingCheckedCount>> {
    return this.api
      .getPendingCheckedCount()
      .pipe(map((response: GeneralResponse<PendingCheckedCount>) => response));
  }

  createSaleInvoice(
    saleInvoiceDTO: SaleInvoiceRequestDTO
  ): Observable<GeneralResponse<AddUpdateResponseDTO>> {
    return this.api
      .create(saleInvoiceDTO)
      .pipe(map((response: GeneralResponse<AddUpdateResponseDTO>) => response));
  }

  updateSaleInvoice(
    saleInvoiceDTO: SaleInvoiceRequestDTO
  ): Observable<GeneralResponse<AddUpdateResponseDTO>> {
    return this.api
      .update(saleInvoiceDTO)
      .pipe(map((response: GeneralResponse<AddUpdateResponseDTO>) => response));
  }

  deleteSaleInvoice(id: string): Observable<GeneralResponse<string>> {
    return this.api
      .delete(id)
      .pipe(map((response: GeneralResponse<string>) => response));
  }

  checkSaleInvoice(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .check(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  approveSaleInvoice(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .approve(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  sendSaleInvoice(id: string): Observable<GeneralResponse<number>> {
    return this.api
      .send(id)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  unpostSaleInvoice(id: string, status: number): Observable<GeneralResponse<number>> {
    return this.api
      .unpost(id, status)
      .pipe(map((response: GeneralResponse<number>) => response));
  }

  getCustomerMonthlyDiscountReport(
    request: CustomerDiscountReportSearchRequestDTO
  ): Observable<CustomerDiscountReportResponseDTO[]> {
    return this.api
      .getCustomerMonthlyDiscountReport(request)
      .pipe(map((response: CustomerDiscountReportResponseDTO[]) => response));
  }
}
