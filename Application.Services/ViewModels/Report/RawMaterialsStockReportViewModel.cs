using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Report
{
    public class RawMaterialsStockReportViewModel
    {
        public string? ProductName { get; set; }
        public string? ProductTypeName { get; set; }
        public Guid? ProductTypeId { get; set; }
        public string? Code { get; set; }
        public string? MeasurementUnitName { get; set; }
        public decimal OpeningQty { get; set; }
        public decimal OpeningValue { get; set; }
        public decimal PurchaseQty { get; set; }
        public decimal AdjustmentInQty { get; set; }
        public decimal TransferReceiveQty { get; set; }
        public decimal InValue { get; set; }
        public decimal IssueQty { get; set; }
        public decimal TransferIssueQty { get; set; }
        public decimal PurchaseReturnQty { get; set; }
        public decimal AdjustmentOutQty { get; set; }
        public decimal OutValue { get; set; }
        public decimal ClosingQty { get; set; }
        public decimal ClosingValue { get; set; }
    }
}
