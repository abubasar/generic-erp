namespace Application.Services.Dtos.Sale.SaleReturn
{
    public class SaleReturnDetailCreationDto
    {
        public Guid ProductId { get; set; }
        public int BagWeight { get; set; }
        public int PrimaryQuantity { get; set; }
        public int Quantity { get; set; }
        public int PrimaryBonusQuantity { get; set; }
        public int BonusQuantity { get; set; }
        public decimal Rate { get; set; }
        public int ReturnPrimaryQuantity { get; set; }
        public int ReturnQuantity { get; set; }
        public int ReturnPrimaryBonusQuantity { get; set; }
        public int ReturnBonusQuantity { get; set; }
        public decimal Amount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal PercentageDiscountAmount { get; set; }
        public decimal OtherDiscountPerUnit { get; set; }
        public decimal VatPercentage { get; set; }
    }
}
