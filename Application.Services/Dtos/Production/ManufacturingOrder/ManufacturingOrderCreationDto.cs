namespace Application.Services.Dtos.Production.ManufacturingOrder
{
    public class ManufacturingOrderCreationDto
    {
        public string? BomNo { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string? FormulationNo { get; set; }
        public Guid FinishedProductId { get; set; }
        public int ProductionQuantity { get; set; }
        public Guid RawMaterialStoreId { get; set; }
        public decimal TotalRmused { get; set; }
        public decimal TotalCost { get; set; }
        public string? Remark { get; set; }

        public virtual ICollection<ManufacturingOrderDetailCreationDto>? ManufacturingOrderDetails { get; set; }
    }
}
