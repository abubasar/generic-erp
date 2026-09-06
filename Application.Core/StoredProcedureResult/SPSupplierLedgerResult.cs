using System.ComponentModel.DataAnnotations.Schema;

namespace Application.Core.StoredProcedureResult
{
    public class SPSupplierLedgerResult
    {
        public Guid SupplierId { get; set; }
        public DateTime BillDate { get; set; }
        public string? BillNo { get; set; }
        public string? Particular { get; set; }
        public string? Po { get; set; }
        public int? Qty { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Amount { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        [NotMapped]
        public decimal Balance { get; set; }
    }
}
