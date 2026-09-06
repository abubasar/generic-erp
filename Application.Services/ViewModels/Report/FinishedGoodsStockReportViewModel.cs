using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Report
{
    public class FinishedGoodsStockReportViewModel
    {
        public string? ProductName { get; set; }
        public string? ProductTypeName { get; set; }
        public string? StoreName { get; set; }
        public string? Code { get; set; }
        public int BagSize { get; set; }
        public decimal OpeningQty { get; set; }
        public decimal OpeningValue { get; set; }
        public decimal ProductionQty { get; set; }
        public decimal TransferReceiveQty { get; set; }
        public decimal SaleReturnQty { get; set; }
        public decimal AdjustmentInQty { get; set; }
        public decimal InValue { get; set; }
        public decimal SaleQty { get; set; }
        public decimal TransferIssueQty { get; set; }
        public decimal AdjustmentOutQty { get; set; }
        public decimal OutValue { get; set; }
        public decimal ClosingQty { get; set; }
        public decimal ClosingValue { get; set; }
    }
}
