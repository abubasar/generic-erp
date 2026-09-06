namespace Application.Services.ViewModels.Report.Purchase
{
    public class PurchaseItemViewModel
    {
        public Guid Id { get; set; }
        public string? BillNo { get; set; }
        public DateTime Date { get; set; }
        public string? SupplierName { get; set; }
        public string? StoreName { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public string? Pono { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Value { get; set; }
    }
    public class PurchaseInvoiceItemViewModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? ProductName { get; set; }
        public string? ProductTypeName { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Value { get; set; }
    }
    public class PurchaseReportViewModel
    {
        public decimal TotalPurchase { get; set; }
        public decimal TotalFreight { get; set; }
        public decimal TotalPurchaseRetun { get; set; }
        public decimal TotalPurchaseDiscount { get; set; }
        public decimal TotalPOPriceAdjustment { get; set; }
    }
}
