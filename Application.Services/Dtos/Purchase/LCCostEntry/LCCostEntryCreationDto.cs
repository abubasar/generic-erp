using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Dtos.Purchase.LCCostEntry
{
    public class LCCostEntryCreationDto
    {
        public Guid PurchaseOrderId { get; set; }
        public string? Ponumber { get; set; }
        public string? LcNumber { get; set; }
        public DateTime EntryDate { get; set; }
        public Guid? CostCenterId { get; set; }
        public decimal Total { get; set; }
        public string? Remark { get; set; }
        public virtual ICollection<LCCostEntryDetailCreationDto>? LccostEntryDetails { get; set; }
    }
}
