

using System.ComponentModel;

namespace Application.Core.Constants
{
    public static class PrimaryPermissions
    {

        [DisplayName("AccessReportModules")]
        [Description("Access Report Modules")]
        public static class AccessReportModules
        {
            public const string PurchaseModuleReports = "Permissions.AccessReportModules.PurchaseModuleReports";
            public const string ProductionModuleReports = "Permissions.AccessReportModules.ProductionModuleReports";
            public const string SalesModuleReports = "Permissions.AccessReportModules.SalesModuleReports";
            public const string PrimarySalesModuleReports = "Permissions.AccessReportModules.PrimarySalesModuleReports";
            public const string AccountsModuleReports = "Permissions.AccessReportModules.AccountsModuleReports";
            public const string InventoryModuleReports = "Permissions.AccessReportModules.InventoryModuleReports";
            public const string PrimaryInventoryModuleReports = "Permissions.AccessReportModules.PrimaryInventoryModuleReports";
        }

        [DisplayName("PrimarySalesModuleReports")]
        [Description("PrimarySalesModuleReports Permissions")]
        public static class PrimarySalesModuleReports
        {
            public const string Primary_Sale_Total_Product_Wise = "Permissions.PrimarySalesModuleReports.Primary_Sale_Total_Product_Wise";
            public const string Primary_Sale_Total_Product_Wise_Multi_Territory = "Permissions.PrimarySalesModuleReports.Primary_Sale_Total_Product_Wise_Multi_Territory";
            public const string Primary_Sale_Total_Product_Wise_Multi_Officer = "Permissions.PrimarySalesModuleReports.Primary_Sale_Total_Product_Wise_Multi_Officer";
            public const string Primary_Customer_Wise_Product_Wise_Sales_Quantity = "Permissions.PrimarySalesModuleReports.Primary_Customer_Wise_Product_Wise_Sales_Quantity";
            public const string Primary_Sale_Aging_Report = "Permissions.PrimarySalesModuleReports.Primary_Sale_Aging_Report";
            public const string Primary_Temporary_Sale_Aging_Report = "Permissions.PrimarySalesModuleReports.Primary_Temporary_Sale_Aging_Report";
            public const string Primary_Customer_Wise_Sales_And_Collection_Report = "Permissions.PrimarySalesModuleReports.Primary_Customer_Wise_Sales_And_Collection_Report";
            public const string Primary_National_Wise_Sales_Collection_And_Due_Report = "Permissions.PrimarySalesModuleReports.Primary_National_Wise_Sales_Collection_And_Due_Report";
            public const string Primary_Marketing_Officer_Wise_Sales_Collection_And_Due_Report = "Permissions.PrimarySalesModuleReports.Primary_Marketing_Officer_Wise_Sales_Collection_And_Due_Report";
            public const string Primary_Marketing_Officer_Wise_Sales_Collection_And_Due_Details_Report = "Permissions.PrimarySalesModuleReports.Primary_Marketing_Officer_Wise_Sales_Collection_And_Due_Details_Report";
            public const string Primary_Marketing_Officer_Wise_Sales_Collection_And_Due_Report_Short = "Permissions.PrimarySalesModuleReports.Primary_Marketing_Officer_Wise_Sales_Collection_And_Due_Report_Short";
            public const string Primary_Month_Wise_Sales_Collection_And_Due_Report = "Permissions.PrimarySalesModuleReports.Primary_Month_Wise_Sales_Collection_And_Due_Report";
        }

        [DisplayName("PrimaryInventoryModuleReports")]
        [Description("PrimaryInventoryModuleReports Permissions")]
        public static class PrimaryInventoryModuleReports
        {
            public const string Primary_Stock_Report = "Permissions.PrimaryInventoryModuleReports.Primary_Stock_Report";
            public const string Primary_Product_Price_List_Report = "Permissions.PrimaryInventoryModuleReports.Primary_Product_Price_List_Report";
            public const string Primary_Finished_Goods_Stock_Report = "Permissions.PrimaryInventoryModuleReports.Primary_Finished_Goods_Stock_Report";
            public const string Primary_Stock_Report_Whole = "Permissions.PrimaryInventoryModuleReports.Primary_Stock_Report_Whole";
        }

        [DisplayName("Territories")]
        [Description("Territories Permissions")]
        public static class Territories
        {
            public const string View = "Permissions.Territories.View";
            public const string Create = "Permissions.Territories.Create";
            public const string Edit = "Permissions.Territories.Edit";
            public const string Delete = "Permissions.Territories.Delete";
        }

        [DisplayName("ReceivePaymentAgainstSales")]
        [Description("ReceivePaymentAgainstSales Permissions")]
        public static class ReceivePaymentAgainstSales
        {
            public const string View = "Permissions.ReceivePaymentAgainstSales.View";
            public const string Create = "Permissions.ReceivePaymentAgainstSales.Create";
            public const string Edit = "Permissions.ReceivePaymentAgainstSales.Edit";
            public const string Delete = "Permissions.ReceivePaymentAgainstSales.Delete";
            public const string Check = "Permissions.ReceivePaymentAgainstSales.Check";
            public const string Approve = "Permissions.ReceivePaymentAgainstSales.Approve";
            public const string Unpost = "Permissions.ReceivePaymentAgainstSales.Unpost";
            public const string SingleFileUpload = "Permissions.ReceivePaymentAgainstSales.SingleFileUpload";
        }

        [DisplayName("Categories")]
        [Description("Categories Permissions")]
        public static class Categories
        {
            public const string View = "Permissions.Categories.View";
            public const string Create = "Permissions.Categories.Create";
            public const string Edit = "Permissions.Categories.Edit";
            public const string Delete = "Permissions.Categories.Delete";
        }

        [DisplayName("PackSizes")]
        [Description("PackSizes Permissions")]
        public static class PackSizes
        {
            public const string View = "Permissions.PackSizes.View";
            public const string Create = "Permissions.PackSizes.Create";
            public const string Edit = "Permissions.PackSizes.Edit";
            public const string Delete = "Permissions.PackSizes.Delete";
        }

        [DisplayName("ProductAudits")]
        [Description("ProductAudits Permissions")]
        public static class ProductAudits
        {
            public const string Product_Audit_Report = "Permissions.ProductAudits.Product_Audit_Report";
        }

    }
}