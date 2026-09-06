namespace Application.Services.Dtos.Purchase.GoodsReceiveNote
{
    public class GoodsReceiveNoteCreationDto
    {
        public DateTime Grndate { get; set; }
        public string? Ponumber { get; set; }
        public Guid StoreId { get; set; }
        public Guid? CurrencyId { get; set; }
        public Guid SupplierId { get; set; }
        public string? ChallanNo { get; set; }
        public DateTime? ChallanDate { get; set; }
        public string? TruckNo { get; set; }
        public string? DriverName { get; set; }
        public string? DriverContactNo { get; set; }
        public int? Transport { get; set; }
        public int PaymentTermInDays { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TransportationCost { get; set; }
        public decimal Total { get; set; }
        public decimal TotalGrnAdjustmentAmount { get; set; }
        public decimal PurchaseOrderTotal { get; set; }
        public bool IsImportPurchase { get; set; }
        public string? ProformaInvoiceNo { get; set; }
        public string? LcNumber { get; set; }
        public decimal ExchangeRate { get; set; }
        public int ImportPurchaseIncoTerm { get; set; }
        public int ImportPurchasePaymentTerm { get; set; }
        public string? PortOfLoading { get; set; }
        public string? PortOfDestination { get; set; }
        public string? Remark { get; set; }

        public virtual ICollection<GoodsReceiveNoteDetailCreationDto>? GoodsReceiveNoteDetails { get; set; }
    }
}
