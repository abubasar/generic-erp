using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Report.Purchase
{
    public class GrnItemViewModel
    {
        public Guid Id { get; set; }
        public string? GrnNo { get; set; }
        public DateTime GrnDate { get; set; }
        public string? SupplierName { get; set; }
        public string? StoreName { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public string? Pono { get; set; }
        public int GrnQuantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal TransportationRate { get; set; }
        public decimal AdditionalLandedCostRate { get; set; }
    }
}
