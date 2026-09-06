namespace Application.Services.Dtos.Inventory.StockTransfer
{
    public class StockTransferUpdateDto : StockTransferCreationDto
    {
        public Guid Id { get; set; }
        public string DeletedStockTransferDetailIds { get; set; } = string.Empty;
        public new ICollection<StockTransferDetailUpdateDto>? StockTransferDetails { get; set; }
    }
}
