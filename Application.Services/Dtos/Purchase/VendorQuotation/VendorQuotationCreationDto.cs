namespace Application.Services.Dtos.Purchase.VendorQuotation
{
    public class VendorQuotationCreationDto
    {
        public string RequisitionNo { get; set; } = string.Empty;
        public string? ReferenceNo { get; set; }
        public Guid StoreId { get; set; }
        public Guid SupplierId { get; set; }
        public int Transport { get; set; }
        public int PaymentMode { get; set; }
        public int PaymentTermInDays { get; set; }
        public int DeliveryTermInDays { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? TermAndCondition { get; set; }
        public Guid? CurrencyId { get; set; }
        public int ImportPurchaseIncoTerm { get; set; }
        public int ImportPurchasePaymentTerm { get; set; }
        public string? Remark { get; set; }
        public virtual ICollection<VendorQuotationDetailCreationDto>? VendorQuotationDetails { get; set; }
    }
}
