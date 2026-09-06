using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Inventory
{
    public class StockTransferDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid StockTransferId { get; set; }
        public Guid ProductId { get; set; }
        public int TransferBagQuantity { get; set; }
        public decimal TransferQuantity { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual ProductViewModel? Product { get; set; }
    }
}
