namespace Application.Services.Dtos.Production.BillOfMaterial
{
    public class BillOfMaterialCreationDto
    {
        public Guid FinishedProductId { get; set; }
        public string? CopiedFromBomNo { get; set; }
        public string? FormulationNo { get; set; }
        public int DosageQuantity { get; set; }
        public decimal TotalQuantity { get; set; }
        public string? Remark { get; set; }
        public virtual ICollection<BillOfMaterialDetailCreationDto>? BillOfMaterialDetails { get; set; }
    }
}
