/*
 * Phase 0 — leading TenantId index on every tenant-scoped table.
 *
 * DB-first project (EF Power Tools), so schema changes are SQL, not migrations.
 * Idempotent: each statement is guarded by index name. Safe to re-run.
 *
 * These are single-column (TenantId) indexes to back the automatic query filter's
 * `WHERE TenantId = @p`. For hot tables, REPLACE the single-column index with a
 * composite `(TenantId, <the columns that table is usually filtered/ordered by>)`
 * — e.g. IX_ReceivePayment_TenantId_Deleted already does this. Review before
 * running in production; skip the tiny lookup tables if you like (Country,
 * Currency, Zone, Region, PackSize, ...).
 *
 * Generated 2026-09-07 from generic-erp-db.
 */
SET NOCOUNT ON;
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Account_TenantId' AND object_id = OBJECT_ID(N'dbo.Account'))
    CREATE NONCLUSTERED INDEX [IX_Account_TenantId] ON [dbo].[Account] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_AccountType_TenantId' AND object_id = OBJECT_ID(N'dbo.AccountType'))
    CREATE NONCLUSTERED INDEX [IX_AccountType_TenantId] ON [dbo].[AccountType] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Area_TenantId' AND object_id = OBJECT_ID(N'dbo.Area'))
    CREATE NONCLUSTERED INDEX [IX_Area_TenantId] ON [dbo].[Area] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BankAccount_TenantId' AND object_id = OBJECT_ID(N'dbo.BankAccount'))
    CREATE NONCLUSTERED INDEX [IX_BankAccount_TenantId] ON [dbo].[BankAccount] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BillOfMaterial_TenantId' AND object_id = OBJECT_ID(N'dbo.BillOfMaterial'))
    CREATE NONCLUSTERED INDEX [IX_BillOfMaterial_TenantId] ON [dbo].[BillOfMaterial] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BillOfMaterialDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.BillOfMaterialDetail'))
    CREATE NONCLUSTERED INDEX [IX_BillOfMaterialDetail_TenantId] ON [dbo].[BillOfMaterialDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Category_TenantId' AND object_id = OBJECT_ID(N'dbo.Category'))
    CREATE NONCLUSTERED INDEX [IX_Category_TenantId] ON [dbo].[Category] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Company_TenantId' AND object_id = OBJECT_ID(N'dbo.Company'))
    CREATE NONCLUSTERED INDEX [IX_Company_TenantId] ON [dbo].[Company] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CostCenter_TenantId' AND object_id = OBJECT_ID(N'dbo.CostCenter'))
    CREATE NONCLUSTERED INDEX [IX_CostCenter_TenantId] ON [dbo].[CostCenter] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Country_TenantId' AND object_id = OBJECT_ID(N'dbo.Country'))
    CREATE NONCLUSTERED INDEX [IX_Country_TenantId] ON [dbo].[Country] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Currency_TenantId' AND object_id = OBJECT_ID(N'dbo.Currency'))
    CREATE NONCLUSTERED INDEX [IX_Currency_TenantId] ON [dbo].[Currency] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CustomerWiseProductDiscount_TenantId' AND object_id = OBJECT_ID(N'dbo.CustomerWiseProductDiscount'))
    CREATE NONCLUSTERED INDEX [IX_CustomerWiseProductDiscount_TenantId] ON [dbo].[CustomerWiseProductDiscount] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CustomerWiseProductDiscountDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.CustomerWiseProductDiscountDetail'))
    CREATE NONCLUSTERED INDEX [IX_CustomerWiseProductDiscountDetail_TenantId] ON [dbo].[CustomerWiseProductDiscountDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_CustomerWiseProductDiscountUsageHistory_TenantId' AND object_id = OBJECT_ID(N'dbo.CustomerWiseProductDiscountUsageHistory'))
    CREATE NONCLUSTERED INDEX [IX_CustomerWiseProductDiscountUsageHistory_TenantId] ON [dbo].[CustomerWiseProductDiscountUsageHistory] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DeliveryNote_TenantId' AND object_id = OBJECT_ID(N'dbo.DeliveryNote'))
    CREATE NONCLUSTERED INDEX [IX_DeliveryNote_TenantId] ON [dbo].[DeliveryNote] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DeliveryNoteDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.DeliveryNoteDetail'))
    CREATE NONCLUSTERED INDEX [IX_DeliveryNoteDetail_TenantId] ON [dbo].[DeliveryNoteDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DeliveryPlace_TenantId' AND object_id = OBJECT_ID(N'dbo.DeliveryPlace'))
    CREATE NONCLUSTERED INDEX [IX_DeliveryPlace_TenantId] ON [dbo].[DeliveryPlace] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Department_TenantId' AND object_id = OBJECT_ID(N'dbo.Department'))
    CREATE NONCLUSTERED INDEX [IX_Department_TenantId] ON [dbo].[Department] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Designation_TenantId' AND object_id = OBJECT_ID(N'dbo.Designation'))
    CREATE NONCLUSTERED INDEX [IX_Designation_TenantId] ON [dbo].[Designation] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DiscountProductWise_TenantId' AND object_id = OBJECT_ID(N'dbo.DiscountProductWise'))
    CREATE NONCLUSTERED INDEX [IX_DiscountProductWise_TenantId] ON [dbo].[DiscountProductWise] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DiscountProductWiseDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.DiscountProductWiseDetail'))
    CREATE NONCLUSTERED INDEX [IX_DiscountProductWiseDetail_TenantId] ON [dbo].[DiscountProductWiseDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_DiscountProductWiseUsageHistory_TenantId' AND object_id = OBJECT_ID(N'dbo.DiscountProductWiseUsageHistory'))
    CREATE NONCLUSTERED INDEX [IX_DiscountProductWiseUsageHistory_TenantId] ON [dbo].[DiscountProductWiseUsageHistory] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EmailAccount_TenantId' AND object_id = OBJECT_ID(N'dbo.EmailAccount'))
    CREATE NONCLUSTERED INDEX [IX_EmailAccount_TenantId] ON [dbo].[EmailAccount] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Employee_TenantId' AND object_id = OBJECT_ID(N'dbo.Employee'))
    CREATE NONCLUSTERED INDEX [IX_Employee_TenantId] ON [dbo].[Employee] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EventLog_TenantId' AND object_id = OBJECT_ID(N'dbo.EventLog'))
    CREATE NONCLUSTERED INDEX [IX_EventLog_TenantId] ON [dbo].[EventLog] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_FinancialYear_TenantId' AND object_id = OBJECT_ID(N'dbo.FinancialYear'))
    CREATE NONCLUSTERED INDEX [IX_FinancialYear_TenantId] ON [dbo].[FinancialYear] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_FundTransfer_TenantId' AND object_id = OBJECT_ID(N'dbo.FundTransfer'))
    CREATE NONCLUSTERED INDEX [IX_FundTransfer_TenantId] ON [dbo].[FundTransfer] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_FundTransferTransactionType_TenantId' AND object_id = OBJECT_ID(N'dbo.FundTransferTransactionType'))
    CREATE NONCLUSTERED INDEX [IX_FundTransferTransactionType_TenantId] ON [dbo].[FundTransferTransactionType] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Generic_TenantId' AND object_id = OBJECT_ID(N'dbo.Generic'))
    CREATE NONCLUSTERED INDEX [IX_Generic_TenantId] ON [dbo].[Generic] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_GoodsReceiveNote_TenantId' AND object_id = OBJECT_ID(N'dbo.GoodsReceiveNote'))
    CREATE NONCLUSTERED INDEX [IX_GoodsReceiveNote_TenantId] ON [dbo].[GoodsReceiveNote] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_GoodsReceiveNoteDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.GoodsReceiveNoteDetail'))
    CREATE NONCLUSTERED INDEX [IX_GoodsReceiveNoteDetail_TenantId] ON [dbo].[GoodsReceiveNoteDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_InventoryType_TenantId' AND object_id = OBJECT_ID(N'dbo.InventoryType'))
    CREATE NONCLUSTERED INDEX [IX_InventoryType_TenantId] ON [dbo].[InventoryType] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_JobLocation_TenantId' AND object_id = OBJECT_ID(N'dbo.JobLocation'))
    CREATE NONCLUSTERED INDEX [IX_JobLocation_TenantId] ON [dbo].[JobLocation] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_JournalEntry_TenantId' AND object_id = OBJECT_ID(N'dbo.JournalEntry'))
    CREATE NONCLUSTERED INDEX [IX_JournalEntry_TenantId] ON [dbo].[JournalEntry] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_JournalEntryDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.JournalEntryDetail'))
    CREATE NONCLUSTERED INDEX [IX_JournalEntryDetail_TenantId] ON [dbo].[JournalEntryDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LcAdjustment_TenantId' AND object_id = OBJECT_ID(N'dbo.LcAdjustment'))
    CREATE NONCLUSTERED INDEX [IX_LcAdjustment_TenantId] ON [dbo].[LcAdjustment] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LcAdjustmentDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.LcAdjustmentDetail'))
    CREATE NONCLUSTERED INDEX [IX_LcAdjustmentDetail_TenantId] ON [dbo].[LcAdjustmentDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LCCostEntry_TenantId' AND object_id = OBJECT_ID(N'dbo.LCCostEntry'))
    CREATE NONCLUSTERED INDEX [IX_LCCostEntry_TenantId] ON [dbo].[LCCostEntry] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_LCCostEntryDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.LCCostEntryDetail'))
    CREATE NONCLUSTERED INDEX [IX_LCCostEntryDetail_TenantId] ON [dbo].[LCCostEntryDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Log_TenantId' AND object_id = OBJECT_ID(N'dbo.Log'))
    CREATE NONCLUSTERED INDEX [IX_Log_TenantId] ON [dbo].[Log] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Machine_TenantId' AND object_id = OBJECT_ID(N'dbo.Machine'))
    CREATE NONCLUSTERED INDEX [IX_Machine_TenantId] ON [dbo].[Machine] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Manufacturer_TenantId' AND object_id = OBJECT_ID(N'dbo.Manufacturer'))
    CREATE NONCLUSTERED INDEX [IX_Manufacturer_TenantId] ON [dbo].[Manufacturer] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ManufacturingOrder_TenantId' AND object_id = OBJECT_ID(N'dbo.ManufacturingOrder'))
    CREATE NONCLUSTERED INDEX [IX_ManufacturingOrder_TenantId] ON [dbo].[ManufacturingOrder] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ManufacturingOrderDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.ManufacturingOrderDetail'))
    CREATE NONCLUSTERED INDEX [IX_ManufacturingOrderDetail_TenantId] ON [dbo].[ManufacturingOrderDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_MeasurementUnit_TenantId' AND object_id = OBJECT_ID(N'dbo.MeasurementUnit'))
    CREATE NONCLUSTERED INDEX [IX_MeasurementUnit_TenantId] ON [dbo].[MeasurementUnit] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Notification_TenantId' AND object_id = OBJECT_ID(N'dbo.Notification'))
    CREATE NONCLUSTERED INDEX [IX_Notification_TenantId] ON [dbo].[Notification] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PackSize_TenantId' AND object_id = OBJECT_ID(N'dbo.PackSize'))
    CREATE NONCLUSTERED INDEX [IX_PackSize_TenantId] ON [dbo].[PackSize] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PaymentMethod_TenantId' AND object_id = OBJECT_ID(N'dbo.PaymentMethod'))
    CREATE NONCLUSTERED INDEX [IX_PaymentMethod_TenantId] ON [dbo].[PaymentMethod] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PaymentMode_TenantId' AND object_id = OBJECT_ID(N'dbo.PaymentMode'))
    CREATE NONCLUSTERED INDEX [IX_PaymentMode_TenantId] ON [dbo].[PaymentMode] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PaymentVoucher_TenantId' AND object_id = OBJECT_ID(N'dbo.PaymentVoucher'))
    CREATE NONCLUSTERED INDEX [IX_PaymentVoucher_TenantId] ON [dbo].[PaymentVoucher] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PaymentVoucherDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.PaymentVoucherDetail'))
    CREATE NONCLUSTERED INDEX [IX_PaymentVoucherDetail_TenantId] ON [dbo].[PaymentVoucherDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Picture_TenantId' AND object_id = OBJECT_ID(N'dbo.Picture'))
    CREATE NONCLUSTERED INDEX [IX_Picture_TenantId] ON [dbo].[Picture] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PoPriceAdjustmentAfterGrn_TenantId' AND object_id = OBJECT_ID(N'dbo.PoPriceAdjustmentAfterGrn'))
    CREATE NONCLUSTERED INDEX [IX_PoPriceAdjustmentAfterGrn_TenantId] ON [dbo].[PoPriceAdjustmentAfterGrn] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PoPriceAdjustmentAfterGrnDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.PoPriceAdjustmentAfterGrnDetail'))
    CREATE NONCLUSTERED INDEX [IX_PoPriceAdjustmentAfterGrnDetail_TenantId] ON [dbo].[PoPriceAdjustmentAfterGrnDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Product_TenantId' AND object_id = OBJECT_ID(N'dbo.Product'))
    CREATE NONCLUSTERED INDEX [IX_Product_TenantId] ON [dbo].[Product] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductAudit_TenantId' AND object_id = OBJECT_ID(N'dbo.ProductAudit'))
    CREATE NONCLUSTERED INDEX [IX_ProductAudit_TenantId] ON [dbo].[ProductAudit] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductCostSetup_TenantId' AND object_id = OBJECT_ID(N'dbo.ProductCostSetup'))
    CREATE NONCLUSTERED INDEX [IX_ProductCostSetup_TenantId] ON [dbo].[ProductCostSetup] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Production_TenantId' AND object_id = OBJECT_ID(N'dbo.Production'))
    CREATE NONCLUSTERED INDEX [IX_Production_TenantId] ON [dbo].[Production] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductionDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.ProductionDetail'))
    CREATE NONCLUSTERED INDEX [IX_ProductionDetail_TenantId] ON [dbo].[ProductionDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ProductType_TenantId' AND object_id = OBJECT_ID(N'dbo.ProductType'))
    CREATE NONCLUSTERED INDEX [IX_ProductType_TenantId] ON [dbo].[ProductType] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseInvoice_TenantId' AND object_id = OBJECT_ID(N'dbo.PurchaseInvoice'))
    CREATE NONCLUSTERED INDEX [IX_PurchaseInvoice_TenantId] ON [dbo].[PurchaseInvoice] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseInvoiceDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.PurchaseInvoiceDetail'))
    CREATE NONCLUSTERED INDEX [IX_PurchaseInvoiceDetail_TenantId] ON [dbo].[PurchaseInvoiceDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseInvoiceSupplierPaymentMapping_TenantId' AND object_id = OBJECT_ID(N'dbo.PurchaseInvoiceSupplierPaymentMapping'))
    CREATE NONCLUSTERED INDEX [IX_PurchaseInvoiceSupplierPaymentMapping_TenantId] ON [dbo].[PurchaseInvoiceSupplierPaymentMapping] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseOrder_TenantId' AND object_id = OBJECT_ID(N'dbo.PurchaseOrder'))
    CREATE NONCLUSTERED INDEX [IX_PurchaseOrder_TenantId] ON [dbo].[PurchaseOrder] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseOrderDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.PurchaseOrderDetail'))
    CREATE NONCLUSTERED INDEX [IX_PurchaseOrderDetail_TenantId] ON [dbo].[PurchaseOrderDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseRequisition_TenantId' AND object_id = OBJECT_ID(N'dbo.PurchaseRequisition'))
    CREATE NONCLUSTERED INDEX [IX_PurchaseRequisition_TenantId] ON [dbo].[PurchaseRequisition] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseRequisitionDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.PurchaseRequisitionDetail'))
    CREATE NONCLUSTERED INDEX [IX_PurchaseRequisitionDetail_TenantId] ON [dbo].[PurchaseRequisitionDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseRequisitionPurchaseOrderMapping_TenantId' AND object_id = OBJECT_ID(N'dbo.PurchaseRequisitionPurchaseOrderMapping'))
    CREATE NONCLUSTERED INDEX [IX_PurchaseRequisitionPurchaseOrderMapping_TenantId] ON [dbo].[PurchaseRequisitionPurchaseOrderMapping] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseReturn_TenantId' AND object_id = OBJECT_ID(N'dbo.PurchaseReturn'))
    CREATE NONCLUSTERED INDEX [IX_PurchaseReturn_TenantId] ON [dbo].[PurchaseReturn] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_PurchaseReturnDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.PurchaseReturnDetail'))
    CREATE NONCLUSTERED INDEX [IX_PurchaseReturnDetail_TenantId] ON [dbo].[PurchaseReturnDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReceivePayment_TenantId' AND object_id = OBJECT_ID(N'dbo.ReceivePayment'))
    CREATE NONCLUSTERED INDEX [IX_ReceivePayment_TenantId] ON [dbo].[ReceivePayment] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReceivePaymentAgainstSale_TenantId' AND object_id = OBJECT_ID(N'dbo.ReceivePaymentAgainstSale'))
    CREATE NONCLUSTERED INDEX [IX_ReceivePaymentAgainstSale_TenantId] ON [dbo].[ReceivePaymentAgainstSale] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReceivePaymentAgainstSaleSaleInvoiceMapping_TenantId' AND object_id = OBJECT_ID(N'dbo.ReceivePaymentAgainstSaleSaleInvoiceMapping'))
    CREATE NONCLUSTERED INDEX [IX_ReceivePaymentAgainstSaleSaleInvoiceMapping_TenantId] ON [dbo].[ReceivePaymentAgainstSaleSaleInvoiceMapping] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReceivePaymentDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.ReceivePaymentDetail'))
    CREATE NONCLUSTERED INDEX [IX_ReceivePaymentDetail_TenantId] ON [dbo].[ReceivePaymentDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReceivePaymentPictureMapping_TenantId' AND object_id = OBJECT_ID(N'dbo.ReceivePaymentPictureMapping'))
    CREATE NONCLUSTERED INDEX [IX_ReceivePaymentPictureMapping_TenantId] ON [dbo].[ReceivePaymentPictureMapping] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReceiveVoucher_TenantId' AND object_id = OBJECT_ID(N'dbo.ReceiveVoucher'))
    CREATE NONCLUSTERED INDEX [IX_ReceiveVoucher_TenantId] ON [dbo].[ReceiveVoucher] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ReceiveVoucherDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.ReceiveVoucherDetail'))
    CREATE NONCLUSTERED INDEX [IX_ReceiveVoucherDetail_TenantId] ON [dbo].[ReceiveVoucherDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RefreshToken_TenantId' AND object_id = OBJECT_ID(N'dbo.RefreshToken'))
    CREATE NONCLUSTERED INDEX [IX_RefreshToken_TenantId] ON [dbo].[RefreshToken] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Region_TenantId' AND object_id = OBJECT_ID(N'dbo.Region'))
    CREATE NONCLUSTERED INDEX [IX_Region_TenantId] ON [dbo].[Region] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RfqSent_TenantId' AND object_id = OBJECT_ID(N'dbo.RfqSent'))
    CREATE NONCLUSTERED INDEX [IX_RfqSent_TenantId] ON [dbo].[RfqSent] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Role_TenantId' AND object_id = OBJECT_ID(N'dbo.Role'))
    CREATE NONCLUSTERED INDEX [IX_Role_TenantId] ON [dbo].[Role] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_RoleClaims_TenantId' AND object_id = OBJECT_ID(N'dbo.RoleClaims'))
    CREATE NONCLUSTERED INDEX [IX_RoleClaims_TenantId] ON [dbo].[RoleClaims] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SaleInvoice_TenantId' AND object_id = OBJECT_ID(N'dbo.SaleInvoice'))
    CREATE NONCLUSTERED INDEX [IX_SaleInvoice_TenantId] ON [dbo].[SaleInvoice] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SaleInvoiceDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.SaleInvoiceDetail'))
    CREATE NONCLUSTERED INDEX [IX_SaleInvoiceDetail_TenantId] ON [dbo].[SaleInvoiceDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SaleOrder_TenantId' AND object_id = OBJECT_ID(N'dbo.SaleOrder'))
    CREATE NONCLUSTERED INDEX [IX_SaleOrder_TenantId] ON [dbo].[SaleOrder] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SaleOrderDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.SaleOrderDetail'))
    CREATE NONCLUSTERED INDEX [IX_SaleOrderDetail_TenantId] ON [dbo].[SaleOrderDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SaleQuotation_TenantId' AND object_id = OBJECT_ID(N'dbo.SaleQuotation'))
    CREATE NONCLUSTERED INDEX [IX_SaleQuotation_TenantId] ON [dbo].[SaleQuotation] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SaleQuotationDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.SaleQuotationDetail'))
    CREATE NONCLUSTERED INDEX [IX_SaleQuotationDetail_TenantId] ON [dbo].[SaleQuotationDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SaleReturn_TenantId' AND object_id = OBJECT_ID(N'dbo.SaleReturn'))
    CREATE NONCLUSTERED INDEX [IX_SaleReturn_TenantId] ON [dbo].[SaleReturn] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SaleReturnDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.SaleReturnDetail'))
    CREATE NONCLUSTERED INDEX [IX_SaleReturnDetail_TenantId] ON [dbo].[SaleReturnDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Shift_TenantId' AND object_id = OBJECT_ID(N'dbo.Shift'))
    CREATE NONCLUSTERED INDEX [IX_Shift_TenantId] ON [dbo].[Shift] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Stock_TenantId' AND object_id = OBJECT_ID(N'dbo.Stock'))
    CREATE NONCLUSTERED INDEX [IX_Stock_TenantId] ON [dbo].[Stock] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StockAdjustment_TenantId' AND object_id = OBJECT_ID(N'dbo.StockAdjustment'))
    CREATE NONCLUSTERED INDEX [IX_StockAdjustment_TenantId] ON [dbo].[StockAdjustment] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StockAdjustmentDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.StockAdjustmentDetail'))
    CREATE NONCLUSTERED INDEX [IX_StockAdjustmentDetail_TenantId] ON [dbo].[StockAdjustmentDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StockTransfer_TenantId' AND object_id = OBJECT_ID(N'dbo.StockTransfer'))
    CREATE NONCLUSTERED INDEX [IX_StockTransfer_TenantId] ON [dbo].[StockTransfer] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_StockTransferDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.StockTransferDetail'))
    CREATE NONCLUSTERED INDEX [IX_StockTransferDetail_TenantId] ON [dbo].[StockTransferDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Store_TenantId' AND object_id = OBJECT_ID(N'dbo.Store'))
    CREATE NONCLUSTERED INDEX [IX_Store_TenantId] ON [dbo].[Store] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SupplierPayment_TenantId' AND object_id = OBJECT_ID(N'dbo.SupplierPayment'))
    CREATE NONCLUSTERED INDEX [IX_SupplierPayment_TenantId] ON [dbo].[SupplierPayment] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SupplierPaymentAgainstPurchase_TenantId' AND object_id = OBJECT_ID(N'dbo.SupplierPaymentAgainstPurchase'))
    CREATE NONCLUSTERED INDEX [IX_SupplierPaymentAgainstPurchase_TenantId] ON [dbo].[SupplierPaymentAgainstPurchase] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SupplierPaymentDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.SupplierPaymentDetail'))
    CREATE NONCLUSTERED INDEX [IX_SupplierPaymentDetail_TenantId] ON [dbo].[SupplierPaymentDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SupplierProductMapping_TenantId' AND object_id = OBJECT_ID(N'dbo.SupplierProductMapping'))
    CREATE NONCLUSTERED INDEX [IX_SupplierProductMapping_TenantId] ON [dbo].[SupplierProductMapping] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SupplierTransactionAgainstPo_TenantId' AND object_id = OBJECT_ID(N'dbo.SupplierTransactionAgainstPo'))
    CREATE NONCLUSTERED INDEX [IX_SupplierTransactionAgainstPo_TenantId] ON [dbo].[SupplierTransactionAgainstPo] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Territory_TenantId' AND object_id = OBJECT_ID(N'dbo.Territory'))
    CREATE NONCLUSTERED INDEX [IX_Territory_TenantId] ON [dbo].[Territory] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Transaction_TenantId' AND object_id = OBJECT_ID(N'dbo.Transaction'))
    CREATE NONCLUSTERED INDEX [IX_Transaction_TenantId] ON [dbo].[Transaction] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_User_TenantId' AND object_id = OBJECT_ID(N'dbo.User'))
    CREATE NONCLUSTERED INDEX [IX_User_TenantId] ON [dbo].[User] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VendorQuotation_TenantId' AND object_id = OBJECT_ID(N'dbo.VendorQuotation'))
    CREATE NONCLUSTERED INDEX [IX_VendorQuotation_TenantId] ON [dbo].[VendorQuotation] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VendorQuotationDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.VendorQuotationDetail'))
    CREATE NONCLUSTERED INDEX [IX_VendorQuotationDetail_TenantId] ON [dbo].[VendorQuotationDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VoucherEntry_TenantId' AND object_id = OBJECT_ID(N'dbo.VoucherEntry'))
    CREATE NONCLUSTERED INDEX [IX_VoucherEntry_TenantId] ON [dbo].[VoucherEntry] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_VoucherEntryDetail_TenantId' AND object_id = OBJECT_ID(N'dbo.VoucherEntryDetail'))
    CREATE NONCLUSTERED INDEX [IX_VoucherEntryDetail_TenantId] ON [dbo].[VoucherEntryDetail] ([TenantId]);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Zone_TenantId' AND object_id = OBJECT_ID(N'dbo.Zone'))
    CREATE NONCLUSTERED INDEX [IX_Zone_TenantId] ON [dbo].[Zone] ([TenantId]);
GO
