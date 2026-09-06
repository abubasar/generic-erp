namespace Application.Services.Dtos.Purchase.PurchaseRequisition
{
    public class PurchaseRequisitionUpdateDto : PurchaseRequisitionCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedPurchaseRequisitionDetailIds { get; set; }
        public new ICollection<PurchaseRequisitionDetailUpdateDto>? PurchaseRequisitionDetails { get; set; }
    }
}
