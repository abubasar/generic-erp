using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Dtos.Purchase.LCCostEntry
{
    public class LCCostEntryDetailUpdateDto : LCCostEntryDetailCreationDto
    {
        public Guid? Id { get; set; }
    }
}
