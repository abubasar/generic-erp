namespace Application.Services.ViewModels.Configuration
{
    public class ProductViewModel
    {

        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid InventoryTypeId { get; set; }
        public Guid ProductTypeId { get; set; }
        public Guid? GenericId { get; set; }
        public string? Composition { get; set; }
        public Guid? CountryId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? PackSizeId { get; set; }
        public bool IsPurchaseProduct { get; set; }
        public bool IsSaleProduct { get; set; }
        public Guid MeasurementUnitId { get; set; }
        public decimal Mrp { get; set; }
        public decimal SalePrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public int AlertQuantity { get; set; }
        public int BagWeight { get; set; }
        public decimal VatPercentage { get; set; }
        public Guid? ManufacturerId { get; set; }
        public decimal LastPurchaseRate { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public InventoryTypeViewModel? InventoryType { get; set; }
        public ProductTypeViewModel? ProductType { get; set; }
        public virtual MeasurementUnitViewModel? MeasurementUnit { get; set; }
        public GenericViewModel? Generic { get; set; }
        public CountryViewModel? Country { get; set; }
        public CategoryViewModel? Category { get; set; }
        public PackSizeViewModel? PackSize { get; set; }
        public ManufacturerViewModel? Manufacturer { get; set; }

    }
}
