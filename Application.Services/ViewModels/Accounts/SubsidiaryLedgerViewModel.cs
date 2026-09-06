namespace Application.Services.ViewModels.Accounts
{
    public class SubsidiaryLedgerViewModel
    {
        public string? Vnumber { get; set; }
        public string? VoucherDate { get; set; }
        public string? AccountName { get; set; }
        public string? Particular { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }

    }
}
