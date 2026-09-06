namespace Application.Services.Dtos.Purchase.PurchaseOrder
{
    public class PurchaseOrderDetailUpdateDto : PurchaseOrderDetailCreationDto
    {
        public Guid? Id { get; set; }
    }
}
