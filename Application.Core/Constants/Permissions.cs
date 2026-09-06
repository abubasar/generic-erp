

using System.ComponentModel;

namespace Application.Core.Constants
{
    public static class Permissions
    {
        [DisplayName("AccessModules")]
        [Description("Access Modules")]
        public static class AccessModules
        {
            public const string Configuration = "Permissions.AccessModules.Configuration";
            public const string Purchase = "Permissions.AccessModules.Purchase";
            public const string Production = "Permissions.AccessModules.Production";
            public const string Inventory = "Permissions.AccessModules.Inventory";
            public const string Sales = "Permissions.AccessModules.Sales";
            public const string Accounts = "Permissions.AccessModules.Accounts";
            public const string Report = "Permissions.AccessModules.Report";
        }

        [DisplayName("Dashboard")]
        [Description("Dashboard Permissions")]
        public static class Dashboard
        {
            public const string DashboardStatistics = "Permissions.Dashboard.DashboardStatistics";
        }

        [DisplayName("Users")]
        [Description("Users Permissions")]
        public static class Users
        {
            public const string View = "Permissions.Users.View";
            public const string Create = "Permissions.Users.Create";
            public const string Edit = "Permissions.Users.Edit";
            public const string Delete = "Permissions.Users.Delete";
        }

        [DisplayName("Roles")]
        [Description("Roles Permissions")]
        public static class Roles
        {
            public const string View = "Permissions.Roles.View";
            public const string Create = "Permissions.Roles.Create";
            public const string Edit = "Permissions.Roles.Edit";
            public const string Delete = "Permissions.Roles.Delete";
            public const string PermissionSetup = "Permissions.Roles.PermissionSetup";
        }

        [DisplayName("Role Claims")]
        [Description("Role Claims Permissions")]
        public static class RoleClaims
        {
            public const string View = "Permissions.RoleClaims.View";
            public const string Create = "Permissions.RoleClaims.Create";
            public const string Edit = "Permissions.RoleClaims.Edit";
            public const string Delete = "Permissions.RoleClaims.Delete";

        }

        [DisplayName("AccountTypes")]
        [Description("AccountTypes Permissions")]
        public static class AccountTypes
        {
            public const string View = "Permissions.AccountTypes.View";
            public const string Create = "Permissions.AccountTypes.Create";
            public const string Edit = "Permissions.AccountTypes.Edit";
            public const string Delete = "Permissions.AccountTypes.Delete";
        }

        [DisplayName("Accounts")]
        [Description("Accounts Permissions")]
        public static class Accounts
        {
            public const string View = "Permissions.Accounts.View";
            public const string Create = "Permissions.Accounts.Create";
            public const string Edit = "Permissions.Accounts.Edit";
            public const string Delete = "Permissions.Accounts.Delete";
        }

        [DisplayName("Companies")]
        [Description("Companies Permissions")]
        public static class Companies
        {
            public const string View = "Permissions.Companies.View";
            public const string Create = "Permissions.Companies.Create";
            public const string Edit = "Permissions.Companies.Edit";
            public const string Delete = "Permissions.Companies.Delete";
        }

        [DisplayName("Countries")]
        [Description("Countries Permissions")]
        public static class Countries
        {
            public const string View = "Permissions.Countries.View";
            public const string Create = "Permissions.Countries.Create";
            public const string Edit = "Permissions.Countries.Edit";
            public const string Delete = "Permissions.Countries.Delete";
        }

        [DisplayName("Departments")]
        [Description("Departments Permissions")]
        public static class Departments
        {
            public const string View = "Permissions.Departments.View";
            public const string Create = "Permissions.Departments.Create";
            public const string Edit = "Permissions.Departments.Edit";
            public const string Delete = "Permissions.Departments.Delete";
        }

        [DisplayName("Shifts")]
        [Description("Shifts Permissions")]
        public static class Shifts
        {
            public const string View = "Permissions.Shifts.View";
            public const string Create = "Permissions.Shifts.Create";
            public const string Edit = "Permissions.Shifts.Edit";
            public const string Delete = "Permissions.Shifts.Delete";
        }

        [DisplayName("Machines")]
        [Description("Machines Permissions")]
        public static class Machines
        {
            public const string View = "Permissions.Machines.View";
            public const string Create = "Permissions.Machines.Create";
            public const string Edit = "Permissions.Machines.Edit";
            public const string Delete = "Permissions.Machines.Delete";
        }

        [DisplayName("Generics")]
        [Description("Generics Permission")]
        public static class Generics
        {
            public const string View = "Permissions.Generics.View";
            public const string Create = "Permissions.Generics.Create";
            public const string Edit = "Permissions.Generics.Edit";
            public const string Delete = "Permissions.Generics.Delete";
        }

        [DisplayName("JobLocations")]
        [Description("JobLocations Permissions")]
        public static class JobLocations
        {
            public const string View = "Permissions.JobLocations.View";
            public const string Create = "Permissions.JobLocations.Create";
            public const string Edit = "Permissions.JobLocations.Edit";
            public const string Delete = "Permissions.JobLocations.Delete";
        }

        [DisplayName("EmailAccounts")]
        [Description("EmailAccounts Permissions")]
        public static class EmailAccounts
        {
            public const string View = "Permissions.EmailAccounts.View";
            public const string Create = "Permissions.EmailAccounts.Create";
            public const string Edit = "Permissions.EmailAccounts.Edit";
            public const string Delete = "Permissions.EmailAccounts.Delete";
        }

        [DisplayName("Regions")]
        [Description("Regions Permissions")]
        public static class Regions
        {
            public const string View = "Permissions.Regions.View";
            public const string Create = "Permissions.Regions.Create";
            public const string Edit = "Permissions.Regions.Edit";
            public const string Delete = "Permissions.Regions.Delete";
        }

        [DisplayName("Zones")]
        [Description("Zones Permissions")]
        public static class Zones
        {
            public const string View = "Permissions.Zones.View";
            public const string Create = "Permissions.Zones.Create";
            public const string Edit = "Permissions.Zones.Edit";
            public const string Delete = "Permissions.Zones.Delete";
        }

        [DisplayName("Areas")]
        [Description("Areas Permissions")]
        public static class Areas
        {
            public const string View = "Permissions.Areas.View";
            public const string Create = "Permissions.Areas.Create";
            public const string Edit = "Permissions.Areas.Edit";
            public const string Delete = "Permissions.Areas.Delete";
        }



        [DisplayName("CostCenters")]
        [Description("CostCenters Permissions")]
        public static class CostCenters
        {
            public const string View = "Permissions.CostCenters.View";
            public const string Create = "Permissions.CostCenters.Create";
            public const string Edit = "Permissions.CostCenters.Edit";
            public const string Delete = "Permissions.CostCenters.Delete";
        }

        [DisplayName("Currencies")]
        [Description("Currencies Permissions")]
        public static class Currencies
        {
            public const string View = "Permissions.Currencies.View";
            public const string Create = "Permissions.Currencies.Create";
            public const string Edit = "Permissions.Currencies.Edit";
            public const string Delete = "Permissions.Currencies.Delete";
        }

        [DisplayName("Designations")]
        [Description("Designations Permissions")]
        public static class Designations
        {
            public const string View = "Permissions.Designations.View";
            public const string Create = "Permissions.Designations.Create";
            public const string Edit = "Permissions.Designations.Edit";
            public const string Delete = "Permissions.Designations.Delete";
        }

        [DisplayName("Employees")]
        [Description("Employees Permissions")]
        public static class Employees
        {
            public const string View = "Permissions.Employees.View";
            public const string Create = "Permissions.Employees.Create";
            public const string Edit = "Permissions.Employees.Edit";
            public const string Delete = "Permissions.Employees.Delete";
        }

        [DisplayName("Stores")]
        [Description("Stores Permissions")]
        public static class Stores
        {
            public const string View = "Permissions.Stores.View";
            public const string Create = "Permissions.Stores.Create";
            public const string Edit = "Permissions.Stores.Edit";
            public const string Delete = "Permissions.Stores.Delete";
        }

        [DisplayName("InventoryTypes")]
        [Description("InventoryTypes Permissions")]
        public static class InventoryTypes
        {
            public const string View = "Permissions.InventoryTypes.View";
            public const string Create = "Permissions.InventoryTypes.Create";
            public const string Edit = "Permissions.InventoryTypes.Edit";
            public const string Delete = "Permissions.InventoryTypes.Delete";
        }

        [DisplayName("ProductTypes")]
        [Description("ProductTypes Permissions")]
        public static class ProductTypes
        {
            public const string View = "Permissions.ProductTypes.View";
            public const string Create = "Permissions.ProductTypes.Create";
            public const string Edit = "Permissions.ProductTypes.Edit";
            public const string Delete = "Permissions.ProductTypes.Delete";
        }

        [DisplayName("MeasurementUnits")]
        [Description("MeasurementUnits Permissions")]
        public static class MeasurementUnits
        {
            public const string View = "Permissions.MeasurementUnits.View";
            public const string Create = "Permissions.MeasurementUnits.Create";
            public const string Edit = "Permissions.MeasurementUnits.Edit";
            public const string Delete = "Permissions.MeasurementUnits.Delete";
        }

        [DisplayName("Products")]
        [Description("Products Permissions")]
        public static class Products
        {
            public const string View = "Permissions.Products.View";
            public const string Create = "Permissions.Products.Create";
            public const string Edit = "Permissions.Products.Edit";
            public const string Delete = "Permissions.Products.Delete";
        }

        [DisplayName("ProductCostSetups")]
        [Description("ProductCostSetups Permissions")]
        public static class ProductCostSetups
        {
            public const string View = "Permissions.ProductCostSetups.View";
            public const string Create = "Permissions.ProductCostSetups.Create";
            public const string Edit = "Permissions.ProductCostSetups.Edit";
            public const string Delete = "Permissions.ProductCostSetups.Delete";
        }

        [DisplayName("PaymentMethods")]
        [Description("PaymentMethods Permissions")]
        public static class PaymentMethods
        {
            public const string View = "Permissions.PaymentMethods.View";
            public const string Create = "Permissions.PaymentMethods.Create";
            public const string Edit = "Permissions.PaymentMethods.Edit";
            public const string Delete = "Permissions.PaymentMethods.Delete";
        }

        [DisplayName("Manufacturers")]
        [Description("Manufacturers Permissions")]
        public static class Manufacturers
        {
            public const string View = "Permissions.Manufacturers.View";
            public const string Create = "Permissions.Manufacturers.Create";
            public const string Edit = "Permissions.Manufacturers.Edit";
            public const string Delete = "Permissions.Manufacturers.Delete";
        }

        [DisplayName("DeliveryPlaces")]
        [Description("DeliveryPlaces Permissions")]
        public static class DeliveryPlaces
        {
            public const string View = "Permissions.DeliveryPlaces.View";
            public const string Create = "Permissions.DeliveryPlaces.Create";
            public const string Edit = "Permissions.DeliveryPlaces.Edit";
            public const string Delete = "Permissions.DeliveryPlaces.Delete";
        }

        [DisplayName("FinancialYears")]
        [Description("FinancialYears Permissions")]
        public static class FinancialYears
        {
            public const string View = "Permissions.FinancialYears.View";
            public const string Create = "Permissions.FinancialYears.Create";
            public const string Edit = "Permissions.FinancialYears.Edit";
            public const string Delete = "Permissions.FinancialYears.Delete";
        }

        [DisplayName("PaymentModes")]
        [Description("PaymentModes Permissions")]
        public static class PaymentModes
        {
            public const string View = "Permissions.PaymentModes.View";
            public const string Create = "Permissions.PaymentModes.Create";
            public const string Edit = "Permissions.PaymentModes.Edit";
            public const string Delete = "Permissions.PaymentModes.Delete";
        }

        [DisplayName("Customers")]
        [Description("Customers Permissions")]
        public static class Customers
        {
            public const string View = "Permissions.Customers.View";
            public const string Create = "Permissions.Customers.Create";
            public const string Edit = "Permissions.Customers.Edit";
            public const string Delete = "Permissions.Customers.Delete";
        }



        [DisplayName("Suppliers")]
        [Description("Suppliers Permissions")]
        public static class Suppliers
        {
            public const string View = "Permissions.Suppliers.View";
            public const string Create = "Permissions.Suppliers.Create";
            public const string Edit = "Permissions.Suppliers.Edit";
            public const string Delete = "Permissions.Suppliers.Delete";
        }

        [DisplayName("PurchaseRequisitions")]
        [Description("PurchaseRequisitions Permissions")]
        public static class PurchaseRequisitions
        {
            public const string View = "Permissions.PurchaseRequisitions.View";
            public const string Create = "Permissions.PurchaseRequisitions.Create";
            public const string Edit = "Permissions.PurchaseRequisitions.Edit";
            public const string Delete = "Permissions.PurchaseRequisitions.Delete";
            public const string Check = "Permissions.PurchaseRequisitions.Check";
            public const string Approve = "Permissions.PurchaseRequisitions.Approve";
            public const string Unpost = "Permissions.PurchaseRequisitions.Unpost";
            public const string SendRFQ = "Permissions.PurchaseRequisitions.SendRFQ";
        }

        [DisplayName("VendorQuotations")]
        [Description("VendorQuotations Permissions")]
        public static class VendorQuotations
        {
            public const string View = "Permissions.VendorQuotations.View";
            public const string Create = "Permissions.VendorQuotations.Create";
            public const string Edit = "Permissions.VendorQuotations.Edit";
            public const string Delete = "Permissions.VendorQuotations.Delete";
            public const string ApproveVendorQuotation = "Permissions.VendorQuotations.ApproveVendorQuotation";
            public const string Unpost = "Permissions.VendorQuotations.Unpost";
        }

        [DisplayName("PurchaseOrders")]
        [Description("PurchaseOrders Permissions")]
        public static class PurchaseOrders
        {
            public const string View = "Permissions.PurchaseOrders.View";
            public const string Create = "Permissions.PurchaseOrders.Create";
            public const string Edit = "Permissions.PurchaseOrders.Edit";
            public const string Delete = "Permissions.PurchaseOrders.Delete";
            public const string Check = "Permissions.PurchaseOrders.Check";
            public const string Approve = "Permissions.PurchaseOrders.Approve";
            public const string SendToSupplier = "Permissions.PurchaseOrders.SendToSupplier";
            public const string Unpost = "Permissions.PurchaseOrders.Unpost";
            public const string Closed = "Permissions.PurchaseOrders.Closed";
            public const string Ready_For_GRN = "Permissions.PurchaseOrders.Ready_For_GRN";
        }

        [DisplayName("GoodsReceiveNotes")]
        [Description("GoodsReceiveNotes Permissions")]
        public static class GoodsReceiveNotes
        {
            public const string View = "Permissions.GoodsReceiveNotes.View";
            public const string Create = "Permissions.GoodsReceiveNotes.Create";
            public const string Edit = "Permissions.GoodsReceiveNotes.Edit";
            public const string Delete = "Permissions.GoodsReceiveNotes.Delete";
            public const string Check = "Permissions.GoodsReceiveNotes.Check";
            public const string Approve = "Permissions.GoodsReceiveNotes.Approve";
            public const string Unpost = "Permissions.GoodsReceiveNotes.Unpost";
        }

        [DisplayName("PurchaseReturns")]
        [Description("PurchaseReturns Permissions")]
        public static class PurchaseReturns
        {
            public const string View = "Permissions.PurchaseReturns.View";
            public const string Create = "Permissions.PurchaseReturns.Create";
            public const string Edit = "Permissions.PurchaseReturns.Edit";
            public const string Delete = "Permissions.PurchaseReturns.Delete";
            public const string Check = "Permissions.PurchaseReturns.Check";
            public const string Approve = "Permissions.PurchaseReturns.Approve";
            public const string SendToSupplier = "Permissions.PurchaseReturns.SendToSupplier";
            public const string Unpost = "Permissions.PurchaseReturns.Unpost";
        }

        [DisplayName("PoPriceAdjustmentAfterGrns")]
        [Description("PoPriceAdjustmentAfterGrns Permissions")]
        public static class PoPriceAdjustmentAfterGrns
        {
            public const string View = "Permissions.PoPriceAdjustmentAfterGrns.View";
            public const string Create = "Permissions.PoPriceAdjustmentAfterGrns.Create";
            public const string Edit = "Permissions.PoPriceAdjustmentAfterGrns.Edit";
            public const string Delete = "Permissions.PoPriceAdjustmentAfterGrns.Delete";
            public const string Check = "Permissions.PoPriceAdjustmentAfterGrns.Check";
            public const string Approve = "Permissions.PoPriceAdjustmentAfterGrns.Approve";
            public const string SendToSupplier = "Permissions.PoPriceAdjustmentAfterGrns.SendToSupplier";
            public const string Unpost = "Permissions.PoPriceAdjustmentAfterGrns.Unpost";
        }

        [DisplayName("PurchaseInvoices")]
        [Description("PurchaseInvoices Permissions")]
        public static class PurchaseInvoices
        {
            public const string View = "Permissions.PurchaseInvoices.View";
            public const string Create = "Permissions.PurchaseInvoices.Create";
            public const string Edit = "Permissions.PurchaseInvoices.Edit";
            public const string Delete = "Permissions.PurchaseInvoices.Delete";
            public const string Check = "Permissions.PurchaseInvoices.Check";
            public const string Approve = "Permissions.PurchaseInvoices.Approve";
            public const string Unpost = "Permissions.PurchaseInvoices.Unpost";
        }

        [DisplayName("StockAdjustments")]
        [Description("StockAdjustments Permissions")]
        public static class StockAdjustments
        {
            public const string View = "Permissions.StockAdjustments.View";
            public const string Create = "Permissions.StockAdjustments.Create";
            public const string Edit = "Permissions.StockAdjustments.Edit";
            public const string Delete = "Permissions.StockAdjustments.Delete";
            public const string Check = "Permissions.StockAdjustments.Check";
            public const string Approve = "Permissions.StockAdjustments.Approve";
            public const string Unpost = "Permissions.StockAdjustments.Unpost";
        }

        [DisplayName("SupplierPaymentAgainstPurchases")]
        [Description("SupplierPaymentAgainstPurchases Permissions")]
        public static class SupplierPaymentAgainstPurchases
        {
            public const string View = "Permissions.SupplierPaymentAgainstPurchases.View";
            public const string Create = "Permissions.SupplierPaymentAgainstPurchases.Create";
            public const string Edit = "Permissions.SupplierPaymentAgainstPurchases.Edit";
            public const string Delete = "Permissions.SupplierPaymentAgainstPurchases.Delete";
            public const string Check = "Permissions.SupplierPaymentAgainstPurchases.Check";
            public const string Approve = "Permissions.SupplierPaymentAgainstPurchases.Approve";
            public const string Unpost = "Permissions.SupplierPaymentAgainstPurchases.Unpost";
        }

        [DisplayName("SupplierPayments")]
        [Description("SupplierPayments Permissions")]
        public static class SupplierPayments
        {
            public const string View = "Permissions.SupplierPayments.View";
            public const string Create = "Permissions.SupplierPayments.Create";
            public const string Edit = "Permissions.SupplierPayments.Edit";
            public const string Delete = "Permissions.SupplierPayments.Delete";
            public const string Check = "Permissions.SupplierPayments.Check";
            public const string Approve = "Permissions.SupplierPayments.Approve";
            public const string Unpost = "Permissions.SupplierPayments.Unpost";
        }

        [DisplayName("BillOfMaterials")]
        [Description("BillOfMaterials Permissions")]
        public static class BillOfMaterials
        {
            public const string View = "Permissions.BillOfMaterials.View";
            public const string Create = "Permissions.BillOfMaterials.Create";
            public const string Edit = "Permissions.BillOfMaterials.Edit";
            public const string Delete = "Permissions.BillOfMaterials.Delete";
            public const string Check = "Permissions.BillOfMaterials.Check";
            public const string Approve = "Permissions.BillOfMaterials.Approve";
            public const string Unpost = "Permissions.BillOfMaterials.Unpost";
        }

        [DisplayName("ManufacturingOrders")]
        [Description("ManufacturingOrders Permissions")]
        public static class ManufacturingOrders
        {
            public const string View = "Permissions.ManufacturingOrders.View";
            public const string Create = "Permissions.ManufacturingOrders.Create";
            public const string Edit = "Permissions.ManufacturingOrders.Edit";
            public const string Delete = "Permissions.ManufacturingOrders.Delete";
            public const string Check = "Permissions.ManufacturingOrders.Check";
            public const string Approve = "Permissions.ManufacturingOrders.Approve";
            public const string Unpost = "Permissions.ManufacturingOrders.Unpost";
            public const string Material_Issue_Summary_Report = "Permissions.ManufacturingOrders.Material_Issue_Summary_Report";
            public const string Material_Issue_Details_Report = "Permissions.ManufacturingOrders.Material_Issue_Details_Report";
        }

        [DisplayName("Productions")]
        [Description("Productions Permissions")]
        public static class Productions
        {
            public const string View = "Permissions.Productions.View";
            public const string Create = "Permissions.Productions.Create";
            public const string Edit = "Permissions.Productions.Edit";
            public const string Delete = "Permissions.Productions.Delete";
            public const string Check = "Permissions.Productions.Check";
            public const string Approve = "Permissions.Productions.Approve";
            public const string Unpost = "Permissions.Productions.Unpost";
            public const string Production_Bill_Summary_Report = "Permissions.Productions.Production_Bill_Summary_Report";
            public const string Production_Bill_Details_Report = "Permissions.Productions.Production_Bill_Details_Report";
        }

        [DisplayName("SaleQuotations")]
        [Description("SaleQuotations Permissions")]
        public static class SaleQuotations
        {
            public const string View = "Permissions.SaleQuotations.View";
            public const string Create = "Permissions.SaleQuotations.Create";
            public const string Edit = "Permissions.SaleQuotations.Edit";
            public const string Delete = "Permissions.SaleQuotations.Delete";
            public const string Check = "Permissions.SaleQuotations.Check";
            public const string Approve = "Permissions.SaleQuotations.Approve";
            public const string Unpost = "Permissions.SaleQuotations.Unpost";
        }

        [DisplayName("SaleOrders")]
        [Description("SaleOrders Permissions")]
        public static class SaleOrders
        {
            public const string View = "Permissions.SaleOrders.View";
            public const string Create = "Permissions.SaleOrders.Create";
            public const string Edit = "Permissions.SaleOrders.Edit";
            public const string Delete = "Permissions.SaleOrders.Delete";
            public const string Check = "Permissions.SaleOrders.Check";
            public const string Approve = "Permissions.SaleOrders.Approve";
            public const string Closed = "Permissions.SaleOrders.Closed";
            public const string Unpost = "Permissions.SaleOrders.Unpost";
        }


        [DisplayName("SaleInvoices")]
        [Description("SaleInvoices Permissions")]
        public static class SaleInvoices
        {
            public const string View = "Permissions.SaleInvoices.View";
            public const string Create = "Permissions.SaleInvoices.Create";
            public const string Edit = "Permissions.SaleInvoices.Edit";
            public const string Delete = "Permissions.SaleInvoices.Delete";
            public const string Check = "Permissions.SaleInvoices.Check";
            public const string Approve = "Permissions.SaleInvoices.Approve";
            public const string Unpost = "Permissions.SaleInvoices.Unpost";
            public const string SendToCustomer = "Permissions.SaleInvoices.SendToCustomer";
            public const string Vat_Invoice_For_Pre_Printed_Document = "Permissions.SaleInvoices.Vat_Invoice_For_Pre_Printed_Document";
        }


        [DisplayName("SaleReturns")]
        [Description("SaleReturns Permissions")]
        public static class SaleReturns
        {
            public const string View = "Permissions.SaleReturns.View";
            public const string Create = "Permissions.SaleReturns.Create";
            public const string Edit = "Permissions.SaleReturns.Edit";
            public const string Delete = "Permissions.SaleReturns.Delete";
            public const string Check = "Permissions.SaleReturns.Check";
            public const string Approve = "Permissions.SaleReturns.Approve";
            public const string Unpost = "Permissions.SaleReturns.Unpost";
        }

        [DisplayName("StockTransfers")]
        [Description("StockTransfers Permissions")]
        public static class StockTransfers
        {
            public const string View = "Permissions.StockTransfers.View";
            public const string Create = "Permissions.StockTransfers.Create";
            public const string Edit = "Permissions.StockTransfers.Edit";
            public const string Delete = "Permissions.StockTransfers.Delete";
            public const string Check = "Permissions.StockTransfers.Check";
            public const string Approve = "Permissions.StockTransfers.Approve";
            public const string Unpost = "Permissions.StockTransfers.Unpost";
        }

        [DisplayName("VoucherEntries")]
        [Description("VoucherEntries Permissions")]
        public static class VoucherEntries
        {
            public const string View = "Permissions.VoucherEntries.View";
            public const string Create = "Permissions.VoucherEntries.Create";
            public const string Edit = "Permissions.VoucherEntries.Edit";
            public const string Delete = "Permissions.VoucherEntries.Delete";
            public const string Check = "Permissions.VoucherEntries.Check";
            public const string Approve = "Permissions.VoucherEntries.Approve";
            public const string Unpost = "Permissions.VoucherEntries.Unpost";
        }

        [DisplayName("JournalEntries")]
        [Description("JournalEntries Permissions")]
        public static class JournalEntries
        {
            public const string View = "Permissions.JournalEntries.View";
            public const string Create = "Permissions.JournalEntries.Create";
            public const string Edit = "Permissions.JournalEntries.Edit";
            public const string Delete = "Permissions.JournalEntries.Delete";
            public const string Check = "Permissions.JournalEntries.Check";
            public const string Approve = "Permissions.JournalEntries.Approve";
            public const string Unpost = "Permissions.JournalEntries.Unpost";
        }

        [DisplayName("FundTransfers")]
        [Description("FundTransfers Permissions")]
        public static class FundTransfers
        {
            public const string View = "Permissions.FundTransfers.View";
            public const string Create = "Permissions.FundTransfers.Create";
            public const string Edit = "Permissions.FundTransfers.Edit";
            public const string Delete = "Permissions.FundTransfers.Delete";
            public const string Check = "Permissions.FundTransfers.Check";
            public const string Approve = "Permissions.FundTransfers.Approve";
            public const string Unpost = "Permissions.FundTransfers.Unpost";
        }

        [DisplayName("PaymentVouchers")]
        [Description("PaymentVouchers Permissions")]
        public static class PaymentVouchers
        {
            public const string View = "Permissions.PaymentVouchers.View";
            public const string Create = "Permissions.PaymentVouchers.Create";
            public const string Edit = "Permissions.PaymentVouchers.Edit";
            public const string Delete = "Permissions.PaymentVouchers.Delete";
            public const string Check = "Permissions.PaymentVouchers.Check";
            public const string Approve = "Permissions.PaymentVouchers.Approve";
            public const string Unpost = "Permissions.PaymentVouchers.Unpost";
        }

        [DisplayName("ReceiveVouchers")]
        [Description("ReceiveVouchers Permissions")]
        public static class ReceiveVouchers
        {
            public const string View = "Permissions.ReceiveVouchers.View";
            public const string Create = "Permissions.ReceiveVouchers.Create";
            public const string Edit = "Permissions.ReceiveVouchers.Edit";
            public const string Delete = "Permissions.ReceiveVouchers.Delete";
            public const string Check = "Permissions.ReceiveVouchers.Check";
            public const string Approve = "Permissions.ReceiveVouchers.Approve";
            public const string Unpost = "Permissions.ReceiveVouchers.Unpost";
        }

        [DisplayName("Reports")]
        [Description("Reports Permissions")]
        public static class Reports
        {
            public const string Stock = "Permissions.Reports.Stock";
        }

        [DisplayName("PurchaseModuleReports")]
        [Description("PurchaseModuleReports Permissions")]
        public static class PurchaseModuleReports
        {
            public const string Purchase_Supplier_Item_Wise = "Permissions.PurchaseModuleReports.Purchase_Supplier_Item_Wise";
            public const string Purchase_Item_Supplier_Wise = "Permissions.PurchaseModuleReports.Purchase_Item_Supplier_Wise";
            public const string Supplier_Ledger_Product_Wise = "Permissions.PurchaseModuleReports.Supplier_Ledger_Product_Wise";
            public const string Purchase_TotalQty_Value_Average_Price = "Permissions.PurchaseModuleReports.Purchase_TotalQty_Value_Average_Price";
            public const string Purchase_Report = "Permissions.PurchaseModuleReports.Purchase_Report";
            public const string GRN_Supplier_Item_Wise = "Permissions.PurchaseModuleReports.GRN_Supplier_Item_Wise";
            public const string GRN_Item_Supplier_Wise = "Permissions.PurchaseModuleReports.GRN_Item_Supplier_Wise";
            public const string GRN_TotalQty_Value_Average_Price = "Permissions.PurchaseModuleReports.GRN_TotalQty_Value_Average_Price";
            public const string CurrentStock_PurchaseRate_And_Quantity = "Permissions.PurchaseModuleReports.CurrentStock_PurchaseRate_And_Quantity";
        }

        [DisplayName("ProductionModuleReports")]
        [Description("ProductionModuleReports Permissions")]
        public static class ProductionModuleReports
        {
            public const string Production_Details = "Permissions.ProductionModuleReports.Production_Details";
            public const string Production_Details2 = "Permissions.ProductionModuleReports.Production_Details2";
            public const string Day_Wise_Production_Summary = "Permissions.ProductionModuleReports.Day_Wise_Production_Summary";
            public const string Day_Wise_Consumption_Qty = "Permissions.ProductionModuleReports.Day_Wise_Consumption_Qty";
            public const string Day_Wise_Consumption_Rate = "Permissions.ProductionModuleReports.Day_Wise_Consumption_Rate";
        }

        [DisplayName("SalesModuleReports")]
        [Description("SalesModuleReports Permissions")]
        public static class SalesModuleReports
        {
            public const string Sales_Report_Warehouse_Wise = "Permissions.SalesModuleReports.Sales_Report_Warehouse_Wise";
            public const string Customer_Discount_Report_Month_Wise = "Permissions.SalesModuleReports.Customer_Discount_Report_Month_Wise";
            public const string Customer_Discount_Report_Date_Wise = "Permissions.SalesModuleReports.Customer_Discount_Report_Date_Wise";
            public const string Sales_Customer_Item_Wise = "Permissions.SalesModuleReports.Sales_Customer_Item_Wise";
            public const string Sales_Item_Customer_Wise = "Permissions.SalesModuleReports.Sales_Item_Customer_Wise";
            public const string Sales_Report_Feed_Total_MO_Wise = "Permissions.SalesModuleReports.Sales_Report_Feed_Total_MO_Wise";
            public const string Sales_Report_Feed_Total_Customer_Wise = "Permissions.SalesModuleReports.Sales_Report_Feed_Total_Customer_Wise";
            public const string Customer_Ledger_Product_Wise = "Permissions.SalesModuleReports.Customer_Ledger_Product_Wise";
            public const string Sales_Report = "Permissions.SalesModuleReports.Sales_Report";
            public const string Sale_Total_Month_Wise = "Permissions.SalesModuleReports.Sale_Total_Month_Wise";
            public const string Sale_Total_Date_Wise = "Permissions.SalesModuleReports.Sale_Total_Date_Wise";
            public const string Sale_Total_Product_Wise = "Permissions.SalesModuleReports.Sale_Total_Product_Wise";
            public const string Sales_Report_Month_Wise = "Permissions.SalesModuleReports.Sales_Report_Month_Wise";
            public const string Customer_Ledger_Feed_Wise = "Permissions.SalesModuleReports.Customer_Ledger_Feed_Wise";
            public const string Transit_Sales_Report = "Permissions.SalesModuleReports.Transit_Sales_Report";
            public const string Sales_Order_History_Report = "Permissions.SalesModuleReports.Sales_Order_History_Report";
        }

        [DisplayName("AccountsModuleReports")]
        [Description("AccountsModuleReports Permissions")]
        public static class AccountsModuleReports
        {
            public const string General_Ledger = "Permissions.AccountsModuleReports.General_Ledger";
            public const string Account_Ledger_Day_Wise = "Permissions.AccountsModuleReports.Account_Ledger_Day_Wise";
            public const string Supplier_Transaction_Report = "Permissions.AccountsModuleReports.Supplier_Transaction_Report";
            public const string Customer_Transaction_Report = "Permissions.AccountsModuleReports.Customer_Transaction_Report";
            public const string Daily_Transaction_Report = "Permissions.AccountsModuleReports.Daily_Transaction_Report";
            public const string Daily_Transaction_Detail_Report = "Permissions.AccountsModuleReports.Daily_Transaction_Detail_Report";
            public const string Cash_Bank_Balance_Report = "Permissions.AccountsModuleReports.Cash_Bank_Balance_Report";
            public const string Cash_Book_Report = "Permissions.AccountsModuleReports.Cash_Book_Report";
            public const string Payment_Report = "Permissions.AccountsModuleReports.Payment_Report";
            public const string Collection_Report = "Permissions.AccountsModuleReports.Collection_Report";
            public const string CogsCalculation_Report = "Permissions.AccountsModuleReports.CogsCalculation_Report";
            public const string Financial_Statement = "Permissions.AccountsModuleReports.Financial_Statement";
        }

        [DisplayName("InventoryModuleReports")]
        [Description("InventoryModuleReports Permissions")]
        public static class InventoryModuleReports
        {
            public const string Item_Wise_Stock_Ledger = "Permissions.InventoryModuleReports.Item_Wise_Stock_Ledger";
            public const string Work_In_Process_Inventory_Stock = "Permissions.InventoryModuleReports.Work_In_Process_Inventory_Stock";
            public const string Low_Stock_Report = "Permissions.InventoryModuleReports.Low_Stock_Report";
            public const string Stock_Report = "Permissions.InventoryModuleReports.Stock_Report";
            public const string Finished_Goods_Stock_Report = "Permissions.InventoryModuleReports.Finished_Goods_Stock_Report";
            public const string Raw_Materials_Stock_Report = "Permissions.InventoryModuleReports.Raw_Materials_Stock_Report";
            public const string Stock_Depot_Product_Wise_Details_Report = "Permissions.InventoryModuleReports.Stock_Depot_Product_Wise_Details_Report";
            public const string Stock_Depot_Product_Wise_Short_Report = "Permissions.InventoryModuleReports.Stock_Depot_Product_Wise_Short_Report";
            public const string Finished_Goods_Stock_Summary_Report = "Permissions.InventoryModuleReports.Finished_Goods_Stock_Summary_Report";
        }

        [DisplayName("FundTransferTransactionTypes")]
        [Description("FundTransferTransactionTypes Permissions")]
        public static class FundTransferTransactionTypes
        {
            public const string View = "Permissions.FundTransferTransactionTypes.View";
            public const string Create = "Permissions.FundTransferTransactionTypes.Create";
            public const string Edit = "Permissions.FundTransferTransactionTypes.Edit";
            public const string Delete = "Permissions.FundTransferTransactionTypes.Delete";
        }

        [DisplayName("Tenants")]
        [Description("Tenants Permissions")]
        public static class Tenants
        {
            public const string View = "Permissions.Tenants.View";
            public const string Create = "Permissions.Tenants.Create";
            public const string Edit = "Permissions.Tenants.Edit";
            public const string Delete = "Permissions.Tenants.Delete";
        }

        [DisplayName("LCCostEntries")]
        [Description("LCCostEntries Permissions")]
        public static class LCCostEntries
        {
            public const string View = "Permissions.LCCostEntries.View";
            public const string Create = "Permissions.LCCostEntries.Create";
            public const string Edit = "Permissions.LCCostEntries.Edit";
            public const string Delete = "Permissions.LCCostEntries.Delete";
            public const string Check = "Permissions.LCCostEntries.Check";
            public const string Approve = "Permissions.LCCostEntries.Approve";
            public const string Unpost = "Permissions.LCCostEntries.Unpost";
            public const string LC_Cost_Entry_Against_PO_Report = "Permissions.LCCostEntries.LC_Cost_Entry_Against_PO_Report";
        }

        [DisplayName("LcAdjustments")]
        [Description("LcAdjustments Permissions")]
        public static class LcAdjustments
        {
            public const string View = "Permissions.LcAdjustments.View";
            public const string Create = "Permissions.LcAdjustments.Create";
            public const string Edit = "Permissions.LcAdjustments.Edit";
            public const string Delete = "Permissions.LcAdjustments.Delete";
            public const string Check = "Permissions.LcAdjustments.Check";
            public const string Approve = "Permissions.LcAdjustments.Approve";
            public const string Unpost = "Permissions.LcAdjustments.Unpost";
        }
        [DisplayName("Maintenances")]
        [Description("Maintenances Permissions")]
        public static class Maintenances
        {
            public const string DownlaodBackup = "Permissions.Maintenances.DownlaodBackup";

        }

    }
}