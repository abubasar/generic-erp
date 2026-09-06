namespace Application.Services.Dtos.Production.Production
{
    public class ProductionCreationDto
    {
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
        public string? Remark { get; set; }

        public virtual ICollection<ProductionDetailCreationDto>? ProductionDetails { get; set; }
    }
}
