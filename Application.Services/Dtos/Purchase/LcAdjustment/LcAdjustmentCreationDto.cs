using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Dtos.Purchase.LcAdjustment
{
    public class LcAdjustmentCreationDto
    {
        public Guid PurchaseInvoiceId { get; set; }
        public string? PurchaseInvoiceNo { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public Guid? CostCenterId { get; set; }
        public decimal InvoiceTotal { get; set; }
        public decimal LcMarginTotal { get; set; }
        public string? Remark { get; set; }

        public virtual ICollection<LcAdjustmentDetailCreationDto>? LcAdjustmentDetails { get; set; }
    }
}
