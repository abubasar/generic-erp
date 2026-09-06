namespace Application.Services.Dtos.Purchase.PurchaseRequisition
{
    public class PurchaseRequisitionView
    {
        public string? Description { get; set; }
        public string? Unit { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal LastPurchaseRate { get; set; }
    }
}
