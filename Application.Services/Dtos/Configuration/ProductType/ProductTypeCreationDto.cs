namespace Application.Services.Dtos.Configuration.ProductType
{
    public class ProductTypeCreationDto
    {
        public Guid InventoryTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
