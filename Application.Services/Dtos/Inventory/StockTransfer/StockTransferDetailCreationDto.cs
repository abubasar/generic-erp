namespace Application.Services.Dtos.Inventory.StockTransfer
{
    public class StockTransferDetailCreationDto
    {
        public Guid ProductId { get; set; }
        public int TransferBagQuantity { get; set; }
        public decimal TransferQuantity { get; set; }
    }
}
