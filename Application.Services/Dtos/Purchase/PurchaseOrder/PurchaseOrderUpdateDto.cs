namespace Application.Services.Dtos.Purchase.PurchaseOrder
{
    public class PurchaseOrderUpdateDto : PurchaseOrderCreationDto
    {
        public Guid Id { get; set; }
        public string DeletedPurchaseOrderDetailIds { get; set; } = string.Empty;
        public new ICollection<PurchaseOrderDetailUpdateDto>? PurchaseOrderDetails { get; set; }
    }
}
