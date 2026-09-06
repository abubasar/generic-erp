using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Accounts.Reports
{
    public class CogsStockLedgerReportViewModel
    {
            public string? ItemName { get; set; }
            public decimal OpeningValue { get; set; }
            public decimal ClosingValue { get; set; }
    }
}
