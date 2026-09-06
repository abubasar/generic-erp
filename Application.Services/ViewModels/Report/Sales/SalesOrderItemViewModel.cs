using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Report.Sales
{
    public class SalesOrderItemViewModel
    {
        public string? StoreName { get; set; }
        public string? CustomerName { get; set; }
        public Guid SaleOrderId { get; set; }
        public string? SaleOrderNo { get; set; }
        public Guid SaleOrderDetailId { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? UnitName { get; set; }
        public int? PrimaryQuantity { get; set; }
        public int? Quantity { get; set; }
        public int? CancelPrimaryQuantity { get; set; }
        public int? CancelQuantity { get; set; }
        public decimal? Rate { get; set; }
    }
}
