namespace Application.Services.Dtos.Purchase.PurchaseRequisition
{
    public class SendRFQtoSelectedSupplierDto
    {
        public SendRFQtoSelectedSupplierDto()
        {
            SelecetedSupplierIds = new List<Guid>();
        }
        public Guid PurchaseRequisitionId { get; set; }
        public List<Guid> SelecetedSupplierIds { get; set; }
    }
}
