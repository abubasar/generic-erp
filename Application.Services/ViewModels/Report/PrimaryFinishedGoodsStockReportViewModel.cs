using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Report
{
    public class PrimaryFinishedGoodsStockReportViewModel
    {
        public string? ProductName { get; set; }
        public string? ProductTypeName { get; set; }
        public string? StoreName { get; set; }
        public string? Code { get; set; }
        public string? PackSize { get; set; }
        public decimal TradePrice { get; set; }
        public decimal OpeningQty { get; set; }
        public decimal OpeningValue { get; set; }
        public decimal ProductionQty { get; set; }
        public decimal PurchaseQty { get; set; }
        public decimal TransferReceiveQty { get; set; }
        public decimal SaleReturnQty { get; set; }
        public decimal ReturnBonusQty { get; set; }
        public decimal AdjustmentInQty { get; set; }
        public decimal InValue { get; set; }
        public decimal SaleQty { get; set; }
        public decimal SaleBonusQty { get; set; }
        public decimal PurchaseRetrunQty { get; set; }
        public decimal TransferIssueQty { get; set; }
        public decimal AdjustmentOutQty { get; set; }
        public decimal OutValue { get; set; }
        public decimal ClosingQty { get; set; }
    }
}
