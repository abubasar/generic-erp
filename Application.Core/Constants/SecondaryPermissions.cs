

using System.ComponentModel;

namespace Application.Core.Constants
{
    public static class SecondaryPermissions
    {
        [DisplayName("AccessReportModules")]
        [Description("Access Report Modules")]
        public static class AccessReportModules
        {
            public const string PurchaseModuleReports = "Permissions.AccessReportModules.PurchaseModuleReports";
            public const string ProductionModuleReports = "Permissions.AccessReportModules.ProductionModuleReports";
            public const string SalesModuleReports = "Permissions.AccessReportModules.SalesModuleReports";
            public const string AccountsModuleReports = "Permissions.AccessReportModules.AccountsModuleReports";
            public const string InventoryModuleReports = "Permissions.AccessReportModules.InventoryModuleReports";
        }

        [DisplayName("CustomerWiseProductDiscounts")]
        [Description("CustomerWiseProductDiscounts Permissions")]
        public static class CustomerWiseProductDiscounts
        {
            public const string View = "Permissions.CustomerWiseProductDiscounts.View";
            public const string Create = "Permissions.CustomerWiseProductDiscounts.Create";
            public const string Edit = "Permissions.CustomerWiseProductDiscounts.Edit";
            public const string Delete = "Permissions.CustomerWiseProductDiscounts.Delete";
            public const string Check = "Permissions.CustomerWiseProductDiscounts.Check";
            public const string Approve = "Permissions.CustomerWiseProductDiscounts.Approve";
            public const string Unpost = "Permissions.CustomerWiseProductDiscounts.Unpost";
        }

        [DisplayName("DiscountProductWises")]
        [Description("DiscountProductWises Permissions")]
        public static class DiscountProductWises
        {
            public const string View = "Permissions.DiscountProductWises.View";
            public const string Create = "Permissions.DiscountProductWises.Create";
            public const string Edit = "Permissions.DiscountProductWises.Edit";
            public const string Delete = "Permissions.DiscountProductWises.Delete";
        }

        [DisplayName("DeliveryNotes")]
        [Description("DeliveryNotes Permissions")]
        public static class DeliveryNotes
        {
            public const string View = "Permissions.DeliveryNotes.View";
            public const string Create = "Permissions.DeliveryNotes.Create";
            public const string Edit = "Permissions.DeliveryNotes.Edit";
            public const string Delete = "Permissions.DeliveryNotes.Delete";
            public const string Check = "Permissions.DeliveryNotes.Check";
            public const string Approve = "Permissions.DeliveryNotes.Approve";
            public const string Unpost = "Permissions.DeliveryNotes.Unpost";
        }

        [DisplayName("ReceivePayments")]
        [Description("ReceivePayments Permissions")]
        public static class ReceivePayments
        {
            public const string View = "Permissions.ReceivePayments.View";
            public const string Create = "Permissions.ReceivePayments.Create";
            public const string Edit = "Permissions.ReceivePayments.Edit";
            public const string Delete = "Permissions.ReceivePayments.Delete";
            public const string Check = "Permissions.ReceivePayments.Check";
            public const string Approve = "Permissions.ReceivePayments.Approve";
            public const string Unpost = "Permissions.ReceivePayments.Unpost";
            public const string SendToCustomer = "Permissions.ReceivePayments.SendToCustomer";
            public const string MultipleFileUpload = "Permissions.ReceivePayments.MultipleFileUpload";
            public const string SingleFileUpload = "Permissions.ReceivePayments.SingleFileUpload";
        }
    }
}