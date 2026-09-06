using Application.Core.Entities;
using Application.Services.ViewModels.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Purchase
{
    public class LCCostEntryDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid LccostEntryId { get; set; }
        public Guid DebitAccountId { get; set; }
        public Guid CreditAccountId { get; set; }
        public decimal Amount { get; set; }
        public bool IsIncludedWithinLandedCost { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual AccountViewModel? CreditAccount { get; set; }
        public virtual AccountViewModel? DebitAccount { get; set; }
    }
}
