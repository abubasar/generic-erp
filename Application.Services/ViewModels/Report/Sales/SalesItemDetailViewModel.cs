namespace Application.Services.ViewModels.Report.Sales
{
    public class SalesItemDetailViewModel
    {
        public Guid? RegionId { get; set; }
        public Guid? ZoneId { get; set; }
        public Guid? AreaId { get; set; }
        public Guid? TerritoryId { get; set; }
        public Guid? MarketingOfficerId { get; set; }
        public string? MarketingOfficerName { get; set; }
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public string? SaleInvoiceNo { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public string? PackSize { get; set; }
        public string? ProductTypeName { get; set; }
        public int DispatchQuantity { get; set; }
        public decimal DispatchValue { get; set; }
        public decimal InvoiceDiscountAmount { get; set; }
        public decimal TradePrice { get; set; }
        public int BonusQuantity { get; set; }
        public int ReturnBonusQuantity { get; set; }
        public int ReturnDispatchQuantity { get; set; }
        public decimal ReturnValue { get; set; }
        public decimal ReturnDiscountAmount { get; set; }
        public decimal TotalVat { get; set; }
    }
}
