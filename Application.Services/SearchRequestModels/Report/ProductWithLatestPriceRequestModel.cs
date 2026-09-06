using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.SearchRequestModels.Report
{
    public class ProductWithLatestPriceRequestModel
    {
        public int ReportType { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? ProductTypeId { get; set; }
    }
}
