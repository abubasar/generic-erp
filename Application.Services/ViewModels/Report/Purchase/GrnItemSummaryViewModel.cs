using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Report.Purchase
{
    public class GrnItemSummaryViewModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? ProductName { get; set; }
        public string? ProductTypeName { get; set; }
        public int GrnQuantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal TransportationRate { get; set; }
        public decimal TransportationCost { get; set; }
    }
}
