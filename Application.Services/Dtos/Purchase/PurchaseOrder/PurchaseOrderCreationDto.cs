namespace Application.Services.Dtos.Purchase.PurchaseOrder
{
    public class PurchaseOrderCreationDto
    {
        public string? RequisitionNo { get; set; }
        public string? QuotationNo { get; set; }
        public string? ReferenceNo { get; set; }
        public DateTime Podate { get; set; }
        public Guid? DeliveryPlaceId { get; set; }
        public Guid? PaymentMethodId { get; set; }
        public Guid StoreId { get; set; }
        public Guid SupplierId { get; set; }
        public string? ProductOrigin { get; set; }
        public string? PackagingType { get; set; }
        public string? ExpiryTime { get; set; }
        public int Transport { get; set; }
        public int PaymentTermInDays { get; set; }
        public int DeliveryTermInDays { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int PaymentMode { get; set; }
        public decimal WeightVariance { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string? TermAndCondition { get; set; }
        public bool IsImportPurchase { get; set; }
        public string? ProformaInvoiceNo { get; set; }
        public string? LcNumber { get; set; }
        public decimal ExchangeRate { get; set; }
        public Guid? CurrencyId { get; set; }
        public int ImportPurchaseIncoTerm { get; set; }
        public int ImportPurchasePaymentTerm { get; set; }
        public string? PortOfLoading { get; set; }
        public string? PortOfDestination { get; set; }
        public string? Remark { get; set; }
        public bool IsPartialDelivery { get; set; }

        public virtual ICollection<PurchaseOrderDetailCreationDto>? PurchaseOrderDetails { get; set; }
    }
}
