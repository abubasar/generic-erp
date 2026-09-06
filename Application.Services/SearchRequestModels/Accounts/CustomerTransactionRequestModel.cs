using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.SearchRequestModels.Accounts
{
    public class CustomerTransactionRequestModel
    {
        public int ReportType { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public Guid? CustomerZoneId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
