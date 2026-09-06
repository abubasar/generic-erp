using Application.Core.Entities;
using Application.Services.ViewModels.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Purchase
{
    public class LcAdjustmentViewModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public Guid PurchaseInvoiceId { get; set; }
        public string? PurchaseInvoiceNo { get; set; }
        public DateTime AdjustmentDate { get; set; }
        public Guid? CostCenterId { get; set; }
        public decimal InvoiceTotal { get; set; }
        public decimal LcMarginTotal { get; set; }
        public string? Remark { get; set; }
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public string? CheckedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public Guid FinancialYearId { get; set; }

        public virtual CostCenterViewModel? CostCenter { get; set; }
        public virtual PurchaseInvoiceViewModel? PurchaseInvoice { get; set; }
        public virtual ICollection<LcAdjustmentDetailViewModel>? LcAdjustmentDetails { get; set; }
    }
}
