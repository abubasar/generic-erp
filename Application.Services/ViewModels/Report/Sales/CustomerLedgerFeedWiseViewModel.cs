using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Report.Sales
{
    public class CustomerLedgerFeedWiseViewModel
    {
        public Guid Id { get; set; }
        public string? InvoiceNo { get; set; }
        public string? DeliveryNoteNo { get; set; }
        public DateTime? Date { get; set; }
        public string? ProductTypeName { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal CommissionRate { get; set; }
        public decimal OfferDiscountPerUnit { get; set; }
        public decimal OtherDiscountPerUnit { get; set; }
        public decimal NetRate { get; set; }
        public decimal RateAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal OfferDiscountAmount { get; set; }
        public decimal OtherDiscountAmount { get; set; }
        public decimal NetRateAmount { get; set; }
        public decimal DepoChargePerKg { get; set; }
    }
}
