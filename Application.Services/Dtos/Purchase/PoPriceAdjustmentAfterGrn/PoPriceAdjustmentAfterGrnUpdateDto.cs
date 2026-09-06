namespace Application.Services.Dtos.Purchase.PoPriceAdjustmentAfterGrn
{
    public class PoPriceAdjustmentAfterGrnUpdateDto : PoPriceAdjustmentAfterGrnCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedPoPriceAdjustmentAfterGrnDetailIds { get; set; }
        public new ICollection<PoPriceAdjustmentAfterGrnDetailUpdateDto>? PoPriceAdjustmentAfterGrnDetails { get; set; }
    }
}
