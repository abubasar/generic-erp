namespace Application.Services.Dtos.Purchase.SupplierPayment
{
    public class SupplierPaymentDetailCreationDto
    {
        public string? PurchaseInvoiceNo { get; set; }
        public Guid AccountId { get; set; }
        public string? AccountDescription { get; set; }
        public decimal Amount { get; set; }
    }
}
