namespace Application.Services.ViewModels.Accounts.Reports
{
    public class SupplierLedgerProductWiseViewModel
    {
        public string? BillNo { get; set; }
        public string? Date { get; set; }
        public string? Description { get; set; }
        public string? PO { get; set; }
        public int Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal Paid { get; set; }
        public decimal Debit { get; set;  }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
    }
}
