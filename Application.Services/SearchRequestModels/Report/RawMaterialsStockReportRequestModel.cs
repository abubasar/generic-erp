using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.SearchRequestModels.Report
{
    public class RawMaterialsStockReportRequestModel
    {
        /* ReportType is being used to indentify Pdf or excel in report design*/
        public int ReportType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? InventoryTypeId { get; set; }
        public Guid? ProductTypeId { get; set; }
    }
}
