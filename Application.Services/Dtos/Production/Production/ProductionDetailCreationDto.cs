namespace Application.Services.Dtos.Production.Production
{
    public class ProductionDetailCreationDto
    {
        public Guid RawMaterialId { get; set; }
        public decimal Quantity { get; set; }
        public decimal AdjustmentQuantity { get; set; }
        public decimal ActualUsedQuantity { get; set; }
        public decimal Amount { get; set; }
    }
}
