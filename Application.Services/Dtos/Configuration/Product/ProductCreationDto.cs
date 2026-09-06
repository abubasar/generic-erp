namespace Application.Services.Dtos.Configuration.Product
{
    public class ProductCreationDto
    {
        public string Name { get; set; } = String.Empty;
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
    }
}
