namespace Application.Services.ViewModels.Report
{
    public class ItemStockLedger
    {
        public string? Date { get; set; }
        public string? Description { get; set; }
        public decimal InQty { get; set; }
        public decimal InRate { get; set; }
        public decimal OutQty { get; set; }
        public decimal OutRate { get; set; }
        public decimal QtyBalance { get; set; }
        public decimal ValueBalance { get; set; }
      
    }
}
