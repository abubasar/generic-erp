using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Dtos.Purchase.LcAdjustment
{
    public class LcAdjustmentDetailUpdateDto : LcAdjustmentDetailCreationDto
    {
        public Guid? Id { get; set; }
    }
}
