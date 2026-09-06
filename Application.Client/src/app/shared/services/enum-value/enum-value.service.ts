import { Injectable } from "@angular/core";

import { map, Observable } from "rxjs";
import { EnumValueApiService } from "../../api/enum-value/enum-value-api.service";
import { ENUM } from "../../models/enum-value/enum.model";

@Injectable({
  providedIn: "root",
})
export class EnumValueService {
  constructor(private api: EnumValueApiService) {}
  //* Priorities
  getPriorities(): Observable<ENUM[]> {
    return this.api
      .getAllPriorities()
      .pipe(map((response: ENUM[]) => response));
  }
  //* RequisitionStatuses
  getRequisitionStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllRequisitionStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  //* MaritalStatues
  getMaritalStatues(): Observable<ENUM[]> {
    return this.api
      .getAllMaritalStatues()
      .pipe(map((response: ENUM[]) => response));
  }
  //* Genders
  getGenders(): Observable<ENUM[]> {
    return this.api.getAllGenders().pipe(map((response: ENUM[]) => response));
  }
  //* BloodGroups
  getBloodGroups(): Observable<ENUM[]> {
    return this.api
      .getAllBloodGroups()
      .pipe(map((response: ENUM[]) => response));
  }
  //* Transports
  getTransports(): Observable<ENUM[]> {
    return this.api
      .getAllTransports()
      .pipe(map((response: ENUM[]) => response));
  }
  //* PaymentModes
  getPaymentModes(): Observable<ENUM[]> {
    return this.api
      .getAllPaymentModes()
      .pipe(map((response: ENUM[]) => response));
  }
  //* PurchaseOrderStatuses
  getPurchaseOrderStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllPurchaseOrderStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  //* GRNStatuses
  getGRNStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllGRNStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  //* PurchaseInvoiceStatuses
  getPurchaseInvoiceStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllPurchaseInvoiceStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  //* PurchaseReturnStatuses
  getPurchaseReturnStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllPurchaseReturnStatuses()
      .pipe(map((response: ENUM[]) => response));
  }

  //* PoPriceAdjustmentAfterGrnStatuses
  getPoPriceAdjustmentAfterGrnStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllPoPriceAdjustmentAfterGrnStatuses()
      .pipe(map((response: ENUM[]) => response));
  }

  //* ImportPurchaseIncoTerms
  getImportPurchaseIncoTerms(): Observable<ENUM[]> {
    return this.api
      .getAllImportPurchaseIncoTerms()
      .pipe(map((response: ENUM[]) => response));
  }

  getImportPurchasePaymentTerms(): Observable<ENUM[]> {
    return this.api
      .getAllImportPurchasePaymentTerms()
      .pipe(map((response: ENUM[]) => response));
  }
  //* BOMStatuses
  getBOMStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllBOMStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  getManufacturingOrderStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllManufacturingOrderStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  getProductionStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllProductionStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  getSaleQuotationStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllSaleQuotationStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  getSaleOrderStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllSaleOrderStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  getSaleReturnStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllSaleReturnStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  getVoucherEntryStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllVoucherEntryStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  getVoucherTypes(): Observable<ENUM[]> {
    return this.api
      .getAllVoucherTypes()
      .pipe(map((response: ENUM[]) => response));
  }
  getJournalEntryStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllJournalEntryStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  getPostTypes(): Observable<ENUM[]> {
    return this.api.getAllPostTypes().pipe(map((response: ENUM[]) => response));
  }

  getDeliveryNoteStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllDeliveryNoteStatuses()
      .pipe(map((response: ENUM[]) => response));
  }

  getSaleInvoiceStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllSaleInvoiceStatuses()
      .pipe(map((response: ENUM[]) => response));
  }

  getStockTransferStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllStockTransferStatuses()
      .pipe(map((response: ENUM[]) => response));
  }

  getEmployeeTypes(): Observable<ENUM[]> {
    return this.api
      .getAllEmployeeTypes()
      .pipe(map((response: ENUM[]) => response));
  }
  getReceivePaymentStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllReceivePaymentStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  getSupplierPaymentStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllSupplierPaymentStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  //* StockAdjustmentStatuses
  getStockAdjustmentStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllStockAdjustmentStatuses()
      .pipe(map((response: ENUM[]) => response));
  }

  //* StockAdjustmentStatuses
  getFundTransferStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllFundTransferStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  //* PaymentVoucherStatuses
  getPaymentVoucherStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllPaymentVoucherStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  //* ReceiveVoucherStatuses
  getReceiveVoucherStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllReceiveVoucherStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  //* SupplierPaymentTypes
  getSupplierPaymentTypes(): Observable<ENUM[]> {
    return this.api
      .getAllSupplierPaymentTypes()
      .pipe(map((response: ENUM[]) => response));
  }
  //* CustomerWiseProductDiscountStatuses
  getCustomerWiseProductDiscountStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllCustomerWiseProductDiscountStatuses()
      .pipe(map((response: ENUM[]) => response));
  }
  //* LCCostEntryStatuses
  getLCCostEntryStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllLcCostEntryStatuses()
      .pipe(map((response: ENUM[]) => response));
  }

  //* LcAdjustmentStatuses
  getLcAdjustmentStatuses(): Observable<ENUM[]> {
    return this.api
      .getAllLcAdjustmentStatuses()
      .pipe(map((response: ENUM[]) => response));
  }

  //* PaymentTerms
  getPaymentTerms(): Observable<ENUM[]> {
    return this.api
      .getAllPaymentTerms()
      .pipe(map((response: ENUM[]) => response));
  }
}
