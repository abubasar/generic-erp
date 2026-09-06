namespace Application.Services.ViewModels.Report.Sales
{
    public class SalesItemViewModel
    {
        public Guid Id { get; set; }
        public string? BillNo { get; set; }
        public DateTime Date { get; set; }
        public string? CustomerName { get; set; }
        public string? StoreName { get; set; }
        public string? ProductTypeName { get; set; }
        public int? BagWeight { get; set; }
        public int? PrimaryQuantity { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public string? SaleOrderNo { get; set; }
        public int Quantity { get; set; }
        public decimal InvoiceDiscountPerUnit { get; set; }
        public decimal CashDiscountPerUnit { get; set; }
        public decimal SpecialDiscountPerUnit { get; set; }
        public decimal OfferDiscountPerUnit { get; set; }
        public decimal OtherDiscountPerUnit { get; set; }
        public decimal MonthlyDiscountPerUnit { get; set; }
        public decimal YearlyDiscountPerUnit { get; set; }
        public decimal TargetDiscountPerUnit { get; set; }
        public decimal Rate { get; set; }
        public decimal InvoiceNetRate { get; set; }
        public decimal NetRate { get; set; }
        public decimal Value { get; set; }
        public decimal NetAmount { get; set; }
    }
    public class SalesReportViewModel
    {
        public Guid Id { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalSalesReturn { get; set; }
        public decimal TotalOfferDiscount { get; set; }
        public decimal TotalOtherDiscount { get; set; }
        public decimal TotalInvoiceDiscount { get; set; }
        public decimal TotalCashDiscount { get; set; }
        public decimal TotalSpecialDiscount { get; set; }
        public decimal TotalMonthlyDiscount { get; set; }
        public decimal TotalYearlyDiscount { get; set; }
        public decimal TotalTargetDiscount { get; set; }
    }
}
