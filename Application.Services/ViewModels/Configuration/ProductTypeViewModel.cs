namespace Application.Services.ViewModels.Configuration
{
    public class ProductTypeViewModel
    {

        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid InventoryTypeId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public virtual InventoryTypeViewModel? InventoryType { get; set; }
    }
}
