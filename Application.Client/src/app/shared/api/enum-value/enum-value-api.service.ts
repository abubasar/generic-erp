import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "environments/environment";

@Injectable({
  providedIn: "root",
})
export class EnumValueApiService {
  baseURL = environment.apiURL + "/EnumValue";

  constructor(private httpClient: HttpClient) {}

  getAllPriorities() {
    return this.httpClient.get(this.baseURL + "/Priorities");
  }
  getAllRequisitionStatuses() {
    return this.httpClient.get(this.baseURL + "/RequisitionStatuses");
  }
  getAllMaritalStatues() {
    return this.httpClient.get(this.baseURL + "/MaritalStatues");
  }
  getAllGenders() {
    return this.httpClient.get(this.baseURL + "/Genders");
  }
  getAllBloodGroups() {
    return this.httpClient.get(this.baseURL + "/BloodGroups");
  }
  getAllTransports() {
    return this.httpClient.get(this.baseURL + "/Transports");
  }
  getAllPaymentModes() {
    return this.httpClient.get(this.baseURL + "/PaymentModes");
  }
  getAllPurchaseOrderStatuses() {
    return this.httpClient.get(this.baseURL + "/PurchaseOrderStatuses");
  }
  getAllGRNStatuses() {
    return this.httpClient.get(this.baseURL + "/GRNStatuses");
  }
  getAllPurchaseInvoiceStatuses() {
    return this.httpClient.get(this.baseURL + "/PurchaseInvoiceStatuses");
  }
  getAllPurchaseReturnStatuses() {
    return this.httpClient.get(this.baseURL + "/PurchaseReturnStatuses");
  }
  getAllPoPriceAdjustmentAfterGrnStatuses() {
    return this.httpClient.get(
      this.baseURL + "/PoPriceAdjustmentAfterGrnStatuses"
    );
  }
  getAllImportPurchaseIncoTerms() {
    return this.httpClient.get(this.baseURL + "/ImportPurchaseIncoTerms");
  }
  getAllImportPurchasePaymentTerms() {
    return this.httpClient.get(this.baseURL + "/ImportPurchasePaymentTerms");
  }
  getAllBOMStatuses() {
    return this.httpClient.get(this.baseURL + "/BOMStatuses");
  }
  getAllManufacturingOrderStatuses() {
    return this.httpClient.get(this.baseURL + "/ManufacturingOrderStatuses");
  }
  getAllProductionStatuses() {
    return this.httpClient.get(this.baseURL + "/ProductionStatuses");
  }
  getAllSaleQuotationStatuses() {
    return this.httpClient.get(this.baseURL + "/SaleQuotationStatuses");
  }
  getAllSaleOrderStatuses() {
    return this.httpClient.get(this.baseURL + "/SaleOrderStatuses");
  }
  getAllSaleReturnStatuses() {
    return this.httpClient.get(this.baseURL + "/SaleReturnStatuses");
  }
  getAllVoucherEntryStatuses() {
    return this.httpClient.get(this.baseURL + "/VoucherEntryStatuses");
  }
  getAllVoucherTypes() {
    return this.httpClient.get(this.baseURL + "/VoucherTypes");
  }
  getAllJournalEntryStatuses() {
    return this.httpClient.get(this.baseURL + "/JournalEntryStatuses");
  }
  getAllPostTypes() {
    return this.httpClient.get(this.baseURL + "/PostTypes");
  }
  getAllDeliveryNoteStatuses() {
    return this.httpClient.get(this.baseURL + "/DeliveryNoteStatuses");
  }
  getAllSaleInvoiceStatuses() {
    return this.httpClient.get(this.baseURL + "/SaleInvoiceStatuses");
  }
  getAllStockTransferStatuses() {
    return this.httpClient.get(this.baseURL + "/StockTransferStatuses");
  }
  getAllEmployeeTypes() {
    return this.httpClient.get(this.baseURL + "/EmployeeTypes");
  }
  getAllReceivePaymentStatuses() {
    return this.httpClient.get(this.baseURL + "/ReceivePaymentStatuses");
  }
  getAllSupplierPaymentStatuses() {
    return this.httpClient.get(this.baseURL + "/SupplierPaymentStatuses");
  }
  getAllStockAdjustmentStatuses() {
    return this.httpClient.get(this.baseURL + "/StockAdjustmentStatuses");
  }
  getAllFundTransferStatuses() {
    return this.httpClient.get(this.baseURL + "/FundTransferStatuses");
  }
  getAllPaymentVoucherStatuses() {
    return this.httpClient.get(this.baseURL + "/PaymentVoucherStatuses");
  }
  getAllReceiveVoucherStatuses() {
    return this.httpClient.get(this.baseURL + "/ReceiveVoucherStatuses");
  }
  getAllSupplierPaymentTypes() {
    return this.httpClient.get(this.baseURL + "/SupplierPaymentTypes");
  }
  getAllCustomerWiseProductDiscountStatuses() {
    return this.httpClient.get(
      this.baseURL + "/CustomerWiseProductDiscountStatuses"
    );
  }
  getAllLcCostEntryStatuses() {
    return this.httpClient.get(this.baseURL + "/LCCostEntryStatuses");
  }
  getAllLcAdjustmentStatuses() {
    return this.httpClient.get(this.baseURL + "/LcAdjustmentStatuses");
  }
  getAllPaymentTerms() {
    return this.httpClient.get(this.baseURL + "/PaymentTerms");
  }
}
