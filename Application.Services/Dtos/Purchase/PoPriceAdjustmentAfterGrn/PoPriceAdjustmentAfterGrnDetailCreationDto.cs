namespace Application.Services.Dtos.Purchase.PoPriceAdjustmentAfterGrn
{
    public class PoPriceAdjustmentAfterGrnDetailCreationDto
    {
        public Guid ProductId { get; set; }
        public int GrnQuantity { get; set; }
        public decimal GrnRate { get; set; }
        public int AdjustmentQuantity { get; set; }
        public decimal AdjustmentRate { get; set; }
        public decimal Amount { get; set; }
    }
}
