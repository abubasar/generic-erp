using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Inventory
{
    public class StockTransferViewModel
    {
        public Guid Id { get; set; }
        public Guid SourceId { get; set; }
        public Guid DestinationId { get; set; }
        public string? TransferNo { get; set; }
        public DateTime TransferDate { get; set; }
        public string? TruckNo { get; set; }
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public string? CheckedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public string? Remark { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public Guid FinancialYearId { get; set; }

        public virtual StoreViewModel? Destination { get; set; }
        public virtual StoreViewModel? Source { get; set; }
        public virtual ICollection<StockTransferDetailViewModel>? StockTransferDetails { get; set; }
    }
}
