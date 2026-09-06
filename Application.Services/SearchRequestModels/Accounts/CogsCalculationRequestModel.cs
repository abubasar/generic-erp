using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.SearchRequestModels.Accounts
{
    public class CogsCalculationRequestModel
    {
        public int? ReportType { get; set; }
        public Guid AccountId { get; set; }
        public Guid FinancialYearId { get; set; }
        public Guid? InventoryTypeId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
