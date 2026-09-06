namespace Application.Services.ViewModels.Accounts.Reports
{
    public class CustomerLedgerProductWiseViewModel
    {
        public string? BillNo { get; set; }
        public string? Date { get; set; }
        public string? Description { get; set; }
        public int Qty { get; set; }
        public decimal TP { get; set; }
        public decimal Commission { get; set; }
        public decimal OfferDiscountPerUnit { get; set; }
        public decimal OtherDiscountPerUnit { get; set; }
        public decimal NetRate { get; set; }
        public decimal TPAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal OfferDiscountAmount { get; set; }
        public decimal OtherDiscountAmount { get; set; }
        public decimal NetRateAmount { get; set; }
        public decimal TransportationCostPerUnit { get; set; }
        public decimal DepoChargePerKg { get; set; }
        public decimal Paid { get; set; }
        public decimal Debit { get; set;  }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
    }
}
