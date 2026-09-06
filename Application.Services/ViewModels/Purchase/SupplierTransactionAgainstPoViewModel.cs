namespace Application.Services.ViewModels.Purchase
{
    public class SupplierTransactionAgainstPoViewModel
    {
        public Guid Id { get; set; }
        public Guid TransactionId { get; set; }
        public string? TransactionDate { get; set; }
        public string? SupplierInvoiceDate { get; set; }
        public int PaymentTermInDays { get; set; }
        public string?  Ponumber { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierTransactionTypeName { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string? Remark { get; set; }
        public Guid? FinancialYearId { get; set; }
    }
}
