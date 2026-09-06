namespace Application.Services.Dtos.Sale.SaleQuotation
{
    public class SaleQuotationDetailCreationDto
    {
        public Guid ProductId { get; set; }
        public int BagWeight { get; set; }
        public int PrimaryQuantity { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountPerUnit { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Amount { get; set; }
    }
}
