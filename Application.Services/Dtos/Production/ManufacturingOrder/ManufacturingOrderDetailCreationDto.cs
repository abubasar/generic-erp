namespace Application.Services.Dtos.Production.ManufacturingOrder
{
    public class ManufacturingOrderDetailCreationDto
    {
        public Guid RawMaterialId { get; set; }
        public decimal StockQuantity { get; set; }
        public decimal Percentage { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
    }
}
