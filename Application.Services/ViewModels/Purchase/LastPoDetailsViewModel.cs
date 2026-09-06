using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Purchase
{
    public class LastPoDetailsViewModel
    {
        public Guid PoId { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? PoNumber { get; set; }
        public int OrderedQuantity { get; set; }
        public int ReceivedQuantity { get; set; }
        public decimal Rate { get; set; }
        public int Status { get; set; }
    }
}
