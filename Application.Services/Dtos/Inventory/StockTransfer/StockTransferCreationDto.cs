namespace Application.Services.Dtos.Inventory.StockTransfer
{
    public class StockTransferCreationDto
    {
        public Guid SourceId { get; set; }
        public Guid DestinationId { get; set; }
        public DateTime TransferDate { get; set; }
        public string? TruckNo { get; set; }
        public string? Remark { get; set; }
        public virtual ICollection<StockTransferDetailCreationDto>? StockTransferDetails { get; set; }
    }
}
