namespace Application.Services.Dtos.Purchase.PurchaseReturn
{
    public class PurchaseReturnUpdateDto : PurchaseReturnCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedPurchaseReturnDetailIds { get; set; }
        public new ICollection<PurchaseReturnDetailUpdateDto>? PurchaseReturnDetails { get; set; }
    }
}
