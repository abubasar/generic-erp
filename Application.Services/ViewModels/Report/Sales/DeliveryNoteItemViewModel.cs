using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Report.Sales
{
    public class DeliveryNoteItemViewModel
    {
        public string? StoreName { get; set; }
        public string? CustomerName { get; set; }
        public string? SaleOrderNo { get; set; }
        public Guid DeliveryNoteId { get; set; }
        public Guid DeliveryNoteDetailId { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? UnitName { get; set; }
        public int? DeliveryPrimaryQuantity { get; set; }
        public int? DeliveryQuantity { get; set; }
        public decimal? DeliveryRate { get; set; }
    }
}
