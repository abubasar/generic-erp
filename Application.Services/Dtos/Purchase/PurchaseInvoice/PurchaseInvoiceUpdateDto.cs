namespace Application.Services.Dtos.Purchase.PurchaseInvoice
{
    public class PurchaseInvoiceUpdateDto : PurchaseInvoiceCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedPurchaseInvoiceDetailIds { get; set; }
        public new ICollection<PurchaseInvoiceDetailUpdateDto>? PurchaseInvoiceDetails { get; set; }
    }
}
