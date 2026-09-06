using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Dtos.Purchase.LCCostEntry
{
    public class LCCostEntryUpdateDto : LCCostEntryCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedLCCostEntryDetailIds { get; set; }
        public new ICollection<LCCostEntryDetailUpdateDto>? LccostEntryDetails { get; set; }
    }
}
