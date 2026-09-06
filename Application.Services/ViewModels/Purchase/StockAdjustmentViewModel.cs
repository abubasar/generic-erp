using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Purchase
{
    public class StockAdjustmentViewModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public Guid StoreId { get; set; }
        public decimal TotalAdjustmentQty { get; set; }
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public string? CheckedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public string? UnpostedBy { get; set; }
        public string? Remark { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public Guid FinancialYearId { get; set; }

        public virtual StoreViewModel? Store { get; set; }
        public virtual ICollection<StockAdjustmentDetailViewModel>? StockAdjustmentDetails { get; set; }
    }
}
