using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.SearchRequestModels.Report
{
    public class SalesItemDetailRequestModel
    {
        /* ReportType is being used to indentify Pdf or excel in report design*/
        public int? ReportType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? CustomerZoneId { get; set; }
        public Guid? CustomerRegionId { get; set; }
        public Guid? CustomerAreaId { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public Guid? CustomerTerritoryId { get; set; }
    }
}
