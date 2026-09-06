namespace Application.Services.ViewModels.Report.Account
{
    public class PaymentReportViewModel
    {
        public string? Date { get; set; }
        public string? VoucherNo { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? CashBank { get; set; }
        public string? CashBankNo { get; set; }
        public decimal Amount { get; }
    }
}
