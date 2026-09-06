using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Dtos.Purchase.LcAdjustment
{
    public class LcAdjustmentDetailCreationDto
    {
        public Guid AccountId { get; set; }
        public string? AccountDescription { get; set; }
        public int PostType { get; set; }
        public decimal Amount { get; set; }
    }
}
