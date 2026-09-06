using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.SearchRequestModels.Sale
{
    public class CustomerLedgerFeedWiseRequestModel
    {
        public int? ReportType { get; set; }
        public Guid AccountId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
