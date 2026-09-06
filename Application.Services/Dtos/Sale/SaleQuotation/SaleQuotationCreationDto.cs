namespace Application.Services.Dtos.Sale.SaleQuotation
{
    public class SaleQuotationCreationDto
    {
        public DateTime QuotationDate { get; set; }
        public Guid CustomerId { get; set; }
        public string? ReferenceNo { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? TermAndCondition { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public bool IsMailSent { get; set; }
        public string? Remark { get; set; }

        public virtual ICollection<SaleQuotationDetailCreationDto>? SaleQuotationDetails { get; set; }
    }
}
