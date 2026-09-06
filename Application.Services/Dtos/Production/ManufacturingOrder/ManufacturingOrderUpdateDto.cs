namespace Application.Services.Dtos.Production.ManufacturingOrder
{
    public class ManufacturingOrderUpdateDto : ManufacturingOrderCreationDto
    {
        public Guid Id { get; set; }
        public string DeletedManufacturingOrderDetailIds { get; set; } = string.Empty;
        public new ICollection<ManufacturingOrderDetailUpdateDto>? ManufacturingOrderDetails { get; set; }
    }
}
