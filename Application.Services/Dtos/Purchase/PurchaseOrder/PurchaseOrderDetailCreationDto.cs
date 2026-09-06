namespace Application.Services.Dtos.Purchase.PurchaseOrder
{
    public class PurchaseOrderDetailCreationDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal CurrencyRate { get; set; }
        public decimal Rate { get; set; }
        public decimal CurrencyAmount { get; set; }
        public decimal Amount { get; set; }
    }
}
