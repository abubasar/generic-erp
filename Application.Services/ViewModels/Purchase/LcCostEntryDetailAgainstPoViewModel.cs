using Application.Services.ViewModels.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Purchase
{
    public class LcCostEntryDetailAgainstPoViewModel
    {
        public string? LcCostEntryNo { get; set; }
        public DateTime? EntryDate { get; set; }
        public string? DebitAccountName { get; set; }
        public string? CreditAccountName { get; set; }
        public decimal Amount { get; set; }
        public bool IsIncludedWithinLandedCost { get; set; }
        public int Status { get; set; }
    }
}
