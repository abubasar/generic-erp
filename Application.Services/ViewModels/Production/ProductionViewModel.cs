using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Production
{
    public class ProductionViewModel
    {
        public Guid Id { get; set; }
        public string? ProductionNo { get; set; }
        public string? ManufacturingOrderNo { get; set; }
        public string? BomNo { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public int BreakTime { get; set; }
        public Guid? ShiftId { get; set; }
        public Guid? MachineId { get; set; }
        public int ExtraDamageQuantity { get; set; }
        public int DustLooseInQuantity { get; set; }
        public int DustLooseOutQuantity { get; set; }
        public Guid FgstoreId { get; set; }
        public decimal TotalRmused { get; set; }
        public decimal RmCost { get; set; }
        public DateTime ProductionDate { get; set; }
        public string? FormulationNo { get; set; }
        public string? BatchNo { get; set; }
        public Guid FinishedProductId { get; set; }
        public int ProductionQuantity { get; set; }
        public int ActualProductionQuantity { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalAdjustmentQuantity { get; set; }
        public decimal TotalAdjustmentCost { get; set; }
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

        public virtual StoreViewModel? Fgstore { get; set; }
        public virtual ProductViewModel? FinishedProduct { get; set; }
        public virtual MachineViewModel? Machine { get; set; }
        public virtual ShiftViewModel? Shift { get; set; }
        public virtual ICollection<ProductionDetailViewModel>? ProductionDetails { get; set; }
    }
}
