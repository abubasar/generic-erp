using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Purchase
{
    public class AvailableBalanceCheckInfo
    {
        public string? PurchaseInvoiceNo { get; set; }
        public string? PurchaseOrderNo { get; set; }
        public Guid PurchaseInvoiceId { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public decimal PayableAmount { get; set; }
    }
}
