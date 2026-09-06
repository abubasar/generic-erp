namespace Application.Services.Dtos.Configuration.Store
{
    public class StoreCreationDto
    {

        public string? Code { get; set; }
        public Guid InventoryTypeId { get; set; }
        public string? Name { get; set; }
        public decimal DepoChargePerKg { get; set; }
    }
}
