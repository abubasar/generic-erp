namespace Application.Services.Dtos.Accounts.AccountsPayable.SupplierPaymentAgainstPurchase
{
    public class SupplierPaymentAgainstPurchaseCreationDto
    {
        public DateTime PaymentDate { get; set; }
        public Guid PurchaseInvoiceId { get; set; }
        public Guid SupplierId { get; set; }
        public decimal Amount { get; set; }
        public Guid CostCenterId { get; set; }
        public Guid FromAccountId { get; set; }
        public string? Remark { get; set; }
    }
}
