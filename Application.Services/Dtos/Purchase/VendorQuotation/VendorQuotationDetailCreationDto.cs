namespace Application.Services.Dtos.Purchase.VendorQuotation
{
    public class VendorQuotationDetailCreationDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }
}
