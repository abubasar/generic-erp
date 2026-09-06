namespace Application.Services.ViewModels.Report
{
    public class StockViewModel
    {
        public Guid InventoryTypeId { get; set; }
        public Guid ProductTypeId { get; set; }
        public Guid ProductId { get; set; }
        public int AlertQuantity { get; set; }
        public Guid StoreId { get; set; }
        public decimal AvailableQty { get; set; }
        public decimal StockValue { get; set; }
        public decimal SalePriceValue { get; set; }
        public virtual string? ProductName { get; set; }
        public virtual string? StoreName { get; set; }
    }
}
