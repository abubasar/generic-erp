namespace Application.Services.Dtos.Production.BillOfMaterial
{
    public class BillOfMaterialDetailCreationDto
    {
        public Guid RawMaterialId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Percentage { get; set; }
    }
}
