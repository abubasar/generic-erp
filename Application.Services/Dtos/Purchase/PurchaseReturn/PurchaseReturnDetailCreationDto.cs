namespace Application.Services.Dtos.Purchase.PurchaseReturn
{
    public class PurchaseReturnDetailCreationDto
    {
        public Guid ProductId { get; set; }
        public int Grnquantity { get; set; }
        public int GrnBagWeightDeductionQty { get; set; }
        public int Quantity { get; set; }
        public int BagWeightDeductionQty { get; set; }
        public int NumberOfBagQuantity { get; set; }
        public int NetQuantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }
}
