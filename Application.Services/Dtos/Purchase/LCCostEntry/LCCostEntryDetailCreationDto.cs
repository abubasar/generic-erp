using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Dtos.Purchase.LCCostEntry
{
    public class LCCostEntryDetailCreationDto
    {
        public Guid DebitAccountId { get; set; }
        public Guid CreditAccountId { get; set; }
        public decimal Amount { get; set; }
        public bool IsIncludedWithinLandedCost { get; set; }
    }
}
