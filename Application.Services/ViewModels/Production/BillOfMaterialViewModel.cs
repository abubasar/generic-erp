using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Production
{
    public class BillOfMaterialViewModel
    {
        public Guid Id { get; set; }
        public string? BomNo { get; set; }
        public string? CopiedFromBomNo { get; set; }
        public Guid FinishedProductId { get; set; }
        public string? FormulationNo { get; set; }
        public int DosageQuantity { get; set; }
        public decimal TotalQuantity { get; set; }
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
        public virtual ICollection<BillOfMaterialDetailViewModel>? BillOfMaterialDetails { get; set; }
    }
}
