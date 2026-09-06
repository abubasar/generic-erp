using Application.Services.Dtos.Purchase.LCCostEntry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Dtos.Purchase.LcAdjustment
{
    public class LcAdjustmentUpdateDto : LcAdjustmentCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedLcAdjustmentDetailIds { get; set; }
        public new ICollection<LcAdjustmentDetailUpdateDto>? LcAdjustmentDetails { get; set; }
    }
}
