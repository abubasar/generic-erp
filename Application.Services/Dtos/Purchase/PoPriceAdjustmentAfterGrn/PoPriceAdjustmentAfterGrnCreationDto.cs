namespace Application.Services.Dtos.Purchase.PoPriceAdjustmentAfterGrn
{
    public class PoPriceAdjustmentAfterGrnCreationDto
    {
        public DateTime AdjustmentDate { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Grnno { get; set; }
        public string? Ponumber { get; set; }
        public Guid SupplierId { get; set; }
        public Guid StoreId { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Remark { get; set; }
        public virtual ICollection<PoPriceAdjustmentAfterGrnDetailCreationDto>? PoPriceAdjustmentAfterGrnDetails { get; set; }
    }
}
