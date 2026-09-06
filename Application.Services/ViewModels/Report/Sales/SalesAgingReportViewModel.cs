using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Report.Sales
{
    public class SalesAgingReportViewModel
    {
        public Guid? RegionId { get; set; }
        public Guid? ZoneId { get; set; }
        public Guid? AreaId { get; set; }
        public Guid? TerritoryId { get; set; }
        public Guid? MarketingOfficerId { get; set; }
        public string? MarketingOfficerName { get; set; }
        public Guid? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? SaleInvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int OverDueDays { get; set; }
        public int PaymentTerm { get; set; }
        public decimal DispatchValue { get; set; }
        public decimal Paid { get; set; }
        public decimal AvailableReceivable { get; set; }
        public decimal InvoiceDiscountAmount { get; set; }
        public decimal ReturnValue { get; set; }
        public decimal ReturnDiscountAmount { get; set; }
    }
}
