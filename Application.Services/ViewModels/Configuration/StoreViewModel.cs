namespace Application.Services.ViewModels.Configuration
{
    public class StoreViewModel
    {

        public Guid Id { get; set; }
        public Guid InventoryTypeId { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public decimal DepoChargePerKg { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public  InventoryTypeViewModel? InventoryType { get; set; }


    }
}
