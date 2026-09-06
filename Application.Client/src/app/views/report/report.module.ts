import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { SharedComponentsModule } from "app/shared/components/shared-components.module";
import { SharedDirectivesModule } from "app/shared/directives/shared-directives.module";
import { SharedMaterialModule } from "app/shared/shared-material.module";
import { AccountsModuleReportComponent } from "./components/accounts-module-report/accounts-module-report.component";
import { AccountsReportComponent } from "./components/accounts-module-report/accounts-report/accounts-report.component";
import { CashBankBalanceReportComponent } from "./components/accounts-module-report/cash-bank-balance-report/cash-bank-balance-report.component";
import { CashBookReportComponent } from "./components/accounts-module-report/cash-book-report/cash-book-report.component";
import { CogsCalculationComponent } from "./components/accounts-module-report/cogs-calculation/cogs-calculation.component";
import { CustomerTransactionReportComponent } from "./components/accounts-module-report/customer-transaction-report/customer-transaction-report.component";
import { DailyTransactionDetailReportComponent } from "./components/accounts-module-report/daily-transaction-detail-report/daily-transaction-detail-report.component";
import { DailyTransactionReportComponent } from "./components/accounts-module-report/daily-transaction-report/daily-transaction-report.component";
import { DayWiseAccountLedgerComponent } from "./components/accounts-module-report/day-wise-account-ledger/day-wise-account-ledger.component";
import { PaymentCollectionReportComponent } from "./components/accounts-module-report/payment-collection-report/payment-collection-report.component";
import { PaymentReportComponent } from "./components/accounts-module-report/payment-report/payment-report.component";
import { SubsidiaryLedgerComponent } from "./components/accounts-module-report/subsidiary-ledger/subsidiary-ledger.component";
import { SupplierTransactionReportComponent } from "./components/accounts-module-report/supplier-transaction-report/supplier-transaction-report.component";
import { ExcelUploadComponent } from "./components/excel-upload/excel-upload.component";
import { FinishedGoodsStockReportComponent } from "./components/inventory-module-report/finished-goods-stock-report/finished-goods-stock-report.component";
import { FinishedGoodsStockSummaryReportComponent } from "./components/inventory-module-report/finished-goods-stock-summary-report/finished-goods-stock-summary-report.component";
import { InventoryModuleReportComponent } from "./components/inventory-module-report/inventory-module-report.component";
import { ItemStockLedgerComponent } from "./components/inventory-module-report/item-stock-ledger/item-stock-ledger.component";
import { LowStockReportComponent } from "./components/inventory-module-report/low-stock-report/low-stock-report.component";
import { RawMaterialsStockReportComponent } from "./components/inventory-module-report/raw-materials-stock-report/raw-materials-stock-report.component";
import { StockDepotProductWiseDetailsReportComponent } from "./components/inventory-module-report/stock-depot-product-wise-details-report/stock-depot-product-wise-details-report.component";
import { StockDepotProductWiseShortReportComponent } from "./components/inventory-module-report/stock-depot-product-wise-short-report/stock-depot-product-wise-short-report.component";
import { StockReportComponent } from "./components/inventory-module-report/stock-report/stock-report.component";
import { WorkInProcessInventoryStockReportComponent } from "./components/inventory-module-report/work-in-process-inventory-stock-report/work-in-process-inventory-stock-report.component";
import { PrimaryFinishedGoodsStockReportComponent } from "./components/primary-inventory-module-report/primary-finished-goods-stock-report/primary-finished-goods-stock-report.component";
import { PrimaryInventoryModuleReportComponent } from "./components/primary-inventory-module-report/primary-inventory-module-report.component";
import { PrimaryProductPriceListReportComponent } from "./components/primary-inventory-module-report/primary-product-price-list-report/primary-product-price-list-report.component";
import { PrimaryStockReportWholeComponent } from "./components/primary-inventory-module-report/primary-stock-report-whole/primary-stock-report-whole.component";
import { PrimaryStockReportComponent } from "./components/primary-inventory-module-report/primary-stock-report/primary-stock-report.component";
import { ProductAuditLogReportComponent } from "./components/primary-inventory-module-report/product-audit-log-report/product-audit-log-report.component";
import { PrimaryAgingReportComponent } from "./components/primary-sales-module-report/primary-aging-report/primary-aging-report.component";
import { PrimaryCustomerWiseProductWiseSalesQuantityReportComponent } from "./components/primary-sales-module-report/primary-customer-wise-product-wise-sales-quantity-report/primary-customer-wise-product-wise-sales-quantity-report.component";
import { PrimaryCustomerWiseSalesAndCollectionReportComponent } from "./components/primary-sales-module-report/primary-customer-wise-sales-and-collection-report/primary-customer-wise-sales-and-collection-report.component";
import { PrimaryMarketingOfficerWiseSalesCollectionAndDueReportShortComponent } from "./components/primary-sales-module-report/primary-marketing-officer-wise-sales-collection-and-due-report-short/primary-marketing-officer-wise-sales-collection-and-due-report-short.component";
import { PrimaryMarketingOfficerWiseSalesCollectionAndDueReportComponent } from "./components/primary-sales-module-report/primary-marketing-officer-wise-sales-collection-and-due-report/primary-marketing-officer-wise-sales-collection-and-due-report.component";
import { PrimaryMonthWiseSalesCollectionAndDueReportComponent } from "./components/primary-sales-module-report/primary-month-wise-sales-collection-and-due-report/primary-month-wise-sales-collection-and-due-report.component";
import { PrimaryNationalWiseSalesCollectionAndDueReportComponent } from "./components/primary-sales-module-report/primary-national-wise-sales-collection-and-due-report/primary-national-wise-sales-collection-and-due-report.component";
import { PrimarySaleTotalProductWiseMultiOfficerReportComponent } from "./components/primary-sales-module-report/primary-sale-total-product-wise-multi-officer-report/primary-sale-total-product-wise-multi-officer-report.component";
import { PrimarySaleTotalProductWiseMultiTerritoryReportComponent } from "./components/primary-sales-module-report/primary-sale-total-product-wise-multi-territory-report/primary-sale-total-product-wise-multi-territory-report.component";
import { PrimarySaleTotalProductWiseReportComponent } from "./components/primary-sales-module-report/primary-sale-total-product-wise-report/primary-sale-total-product-wise-report.component";
import { PrimarySalesModuleReportComponent } from "./components/primary-sales-module-report/primary-sales-module-report.component";
import { DayWiseConsumptionQuantityComponent } from "./components/production-module-report/day-wise-consumption-quantity/day-wise-consumption-quantity.component";
import { DayWiseConsumptionRateComponent } from "./components/production-module-report/day-wise-consumption-rate/day-wise-consumption-rate.component";
import { DayWiseProductionSummaryComponent } from "./components/production-module-report/day-wise-production-summary/day-wise-production-summary.component";
import { ProductionDetailsReportComponent } from "./components/production-module-report/production-details-report/production-details-report.component";
import { ProductionModuleReportComponent } from "./components/production-module-report/production-module-report.component";
import { ProductionReportComponent } from "./components/production-module-report/production-report/production-report.component";
import { CurrentStockPurchaseRateAndQuantityComponent } from "./components/purchase-module-report/current-stock-purchase-rate-and-quantity/current-stock-purchase-rate-and-quantity.component";
import { GrnItemSupplierWiseReportComponent } from "./components/purchase-module-report/grn-item-supplier-wise-report/grn-item-supplier-wise-report.component";
import { GrnSupplierItemWiseReportComponent } from "./components/purchase-module-report/grn-supplier-item-wise-report/grn-supplier-item-wise-report.component";
import { GrnTotalQtyValueAveragePriceReportComponent } from "./components/purchase-module-report/grn-total-qty-value-average-price-report/grn-total-qty-value-average-price-report.component";
import { LccostEntriesAgainstLcNumberReportComponent } from "./components/purchase-module-report/lccost-entries-against-lc-number-report/lccost-entries-against-lc-number-report.component";
import { PurchaseItemWiseSupplierComponent } from "./components/purchase-module-report/purchase-item-wise-supplier/purchase-item-wise-supplier.component";
import { PurchaseModuleReportComponent } from "./components/purchase-module-report/purchase-module-report.component";
import { PurchaseSummaryReportComponent } from "./components/purchase-module-report/purchase-summary-report/purchase-summary-report.component";
import { SupplierLedgerProductWiseReportComponent } from "./components/purchase-module-report/supplier-ledger-product-wise-report/supplier-ledger-product-wise-report.component";
import { SupplierWisePurchaseItemComponent } from "./components/purchase-module-report/supplier-wise-purchase-item/supplier-wise-purchase-item.component";
import { TotalPurchaseItemComponent } from "./components/purchase-module-report/total-purchase-item/total-purchase-item.component";
import { CustomerDateWiseDiscountReportComponent } from "./components/sales-module-report/customer-date-wise-discount-report/customer-date-wise-discount-report.component";
import { CustomerLedgerFeedWiseReportComponent } from "./components/sales-module-report/customer-ledger-feed-wise-report/customer-ledger-feed-wise-report.component";
import { CustomerLedgerProductWiseReportComponent } from "./components/sales-module-report/customer-ledger-product-wise-report/customer-ledger-product-wise-report.component";
import { CustomerMonthlyDiscountReportComponent } from "./components/sales-module-report/customer-monthly-discount-report/customer-monthly-discount-report.component";
import { SaleTotalProductWiseReportComponent } from "./components/sales-module-report/sale-total-product-wise-report/sale-total-product-wise-report.component";
import { SalesCustomerItemWiseComponent } from "./components/sales-module-report/sales-customer-item-wise/sales-customer-item-wise.component";
import { SalesItemCustomerWiseComponent } from "./components/sales-module-report/sales-item-customer-wise/sales-item-customer-wise.component";
import { SalesModuleReportComponent } from "./components/sales-module-report/sales-module-report.component";
import { SalesMonthWiseComponent } from "./components/sales-module-report/sales-month-wise/sales-month-wise.component";
import { SalesOrderHistoryReportComponent } from "./components/sales-module-report/sales-order-history-report/sales-order-history-report.component";
import { SalesReportFeedTotalCustomerWiseComponent } from "./components/sales-module-report/sales-report-feed-total-customer-wise/sales-report-feed-total-customer-wise.component";
import { SalesReportFeedTotalMoWiseComponent } from "./components/sales-module-report/sales-report-feed-total-mo-wise/sales-report-feed-total-mo-wise.component";
import { SalesSummaryReportComponent } from "./components/sales-module-report/sales-summary-report/sales-summary-report.component";
import { SalesTotalDateWiseReportComponent } from "./components/sales-module-report/sales-total-date-wise-report/sales-total-date-wise-report.component";
import { SalesTotalMonthWiseReportComponent } from "./components/sales-module-report/sales-total-month-wise-report/sales-total-month-wise-report.component";
import { SalesWarehouseWiseComponent } from "./components/sales-module-report/sales-warehouse-wise/sales-warehouse-wise.component";
import { TransitSalesReportComponent } from "./components/sales-module-report/transit-sales-report/transit-sales-report.component";
import { ReportRoutingModule } from "./report-routing.module";

@NgModule({
  declarations: [
    StockReportComponent,
    LowStockReportComponent,
    AccountsReportComponent,
    SubsidiaryLedgerComponent,
    CustomerMonthlyDiscountReportComponent,
    WorkInProcessInventoryStockReportComponent,
    ItemStockLedgerComponent,
    DayWiseProductionSummaryComponent,
    SalesModuleReportComponent,
    SalesWarehouseWiseComponent,
    PaymentCollectionReportComponent,
    ExcelUploadComponent,
    AccountsModuleReportComponent,
    PaymentReportComponent,
    DayWiseAccountLedgerComponent,
    SupplierTransactionReportComponent,
    CustomerTransactionReportComponent,
    DailyTransactionReportComponent,
    DailyTransactionDetailReportComponent,
    PurchaseModuleReportComponent,
    SupplierWisePurchaseItemComponent,
    PurchaseItemWiseSupplierComponent,
    SalesReportFeedTotalMoWiseComponent,
    SalesReportFeedTotalCustomerWiseComponent,
    SalesCustomerItemWiseComponent,
    SalesItemCustomerWiseComponent,
    ProductionModuleReportComponent,
    ProductionReportComponent,
    SupplierLedgerProductWiseReportComponent,
    CustomerLedgerProductWiseReportComponent,
    TotalPurchaseItemComponent,
    PurchaseSummaryReportComponent,
    SalesSummaryReportComponent,
    CashBookReportComponent,
    SalesTotalMonthWiseReportComponent,
    ProductionDetailsReportComponent,
    SalesTotalDateWiseReportComponent,
    InventoryModuleReportComponent,
    SaleTotalProductWiseReportComponent,
    CogsCalculationComponent,
    DayWiseConsumptionQuantityComponent,
    DayWiseConsumptionRateComponent,
    SalesMonthWiseComponent,
    FinishedGoodsStockReportComponent,
    RawMaterialsStockReportComponent,
    GrnSupplierItemWiseReportComponent,
    GrnItemSupplierWiseReportComponent,
    GrnTotalQtyValueAveragePriceReportComponent,
    StockDepotProductWiseDetailsReportComponent,
    StockDepotProductWiseShortReportComponent,
    CustomerDateWiseDiscountReportComponent,
    CustomerLedgerFeedWiseReportComponent,
    TransitSalesReportComponent,
    SalesOrderHistoryReportComponent,
    FinishedGoodsStockSummaryReportComponent,
    PrimarySaleTotalProductWiseReportComponent,
    PrimarySalesModuleReportComponent,
    PrimaryAgingReportComponent,
    PrimaryCustomerWiseSalesAndCollectionReportComponent,
    PrimaryNationalWiseSalesCollectionAndDueReportComponent,
    PrimaryInventoryModuleReportComponent,
    PrimaryStockReportComponent,
    PrimaryCustomerWiseProductWiseSalesQuantityReportComponent,
    PrimaryProductPriceListReportComponent,
    PrimarySaleTotalProductWiseMultiTerritoryReportComponent,
    PrimaryFinishedGoodsStockReportComponent,
    PrimaryStockReportWholeComponent,
    PrimarySaleTotalProductWiseMultiOfficerReportComponent,
    LccostEntriesAgainstLcNumberReportComponent,
    ProductAuditLogReportComponent,
    PrimaryMarketingOfficerWiseSalesCollectionAndDueReportComponent,
    PrimaryMarketingOfficerWiseSalesCollectionAndDueReportShortComponent,
    PrimaryMonthWiseSalesCollectionAndDueReportComponent,
    CashBankBalanceReportComponent,
    CurrentStockPurchaseRateAndQuantityComponent,
  ],
  imports: [
    CommonModule,
    ReportRoutingModule,
    SharedMaterialModule,
    SharedComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    SharedDirectivesModule,
  ],
})
export class ReportModule {}
