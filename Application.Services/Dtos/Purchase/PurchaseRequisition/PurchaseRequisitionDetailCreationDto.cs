namespace Application.Services.Dtos.Purchase.PurchaseRequisition
{
    public class PurchaseRequisitionDetailCreationDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal LastPurchaseRate { get; set; }
        public int OrderedQuantity { get; set; }
        public int AlertQuantity { get; set; }
        public decimal CurrentStockQuantity { get; set; }
    }
}
