namespace Application.Services.Dtos.Sale.SaleReturn
{
    public class SaleReturnCreationDto
    {
        public string DeliveryNoteNo { get; set; } = string.Empty;
        public string InvoiceNo { get; set; } = string.Empty;
        public DateTime SaleReturnDate { get; set; }
        public string? ReferenceNo { get; set; }
        public Guid StoreId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public Guid? CustomerTerritoryId { get; set; }
        public decimal TotalVat { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalPercentageDiscountAmount { get; set; }
        public decimal OtherDiscount { get; set; }
        public decimal Total { get; set; }
        public string? Remark { get; set; }
        public virtual ICollection<SaleReturnDetailCreationDto>? SaleReturnDetails { get; set; }
    }
}
