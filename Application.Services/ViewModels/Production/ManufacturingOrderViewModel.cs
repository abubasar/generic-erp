using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Production
{
    public class ManufacturingOrderViewModel
    {
        public Guid Id { get; set; }
        public string? ManufacturingOrderNo { get; set; }
        public string? BomNo { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string? FormulationNo { get; set; }
        public Guid FinishedProductId { get; set; }
        public int ProductionQuantity { get; set; }
        public Guid RawMaterialStoreId { get; set; }
        public decimal TotalRmused { get; set; }
        public decimal RmCost { get; set; }
        public decimal StandardDirectExpenseAmount { get; set; }
        public decimal StandardFactoryOverheadAmount { get; set; }
        public decimal TotalCost { get; set; }
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

        public virtual ProductViewModel? FinishedProduct { get; set; }
        public virtual StoreViewModel? RawMaterialStore { get; set; }
        public virtual ICollection<ManufacturingOrderDetailViewModel>? ManufacturingOrderDetails { get; set; }
    }
}
