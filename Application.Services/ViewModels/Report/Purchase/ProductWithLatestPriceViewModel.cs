using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Report.Purchase
{
    public class ProductWithLatestPriceViewModel
    {
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? MeasurementUnitName { get; set; }
        public string? ProductTypeName { get; set; }
        public DateTime? GrnDate { get; set; }
        public decimal GrnRate { get; set; }
        public decimal LatestRate { get; set; }
        public decimal GrnQuantity { get; set; }
        public decimal BalanceQuantity { get; set; }
    }
}
