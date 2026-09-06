using Application.Services.Dtos.Purchase.LCCostEntry;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Validators.Purchase.LCCostEntry
{
    public class LCCostEntryUpdateDtoValidator : AbstractValidator<LCCostEntryUpdateDto>
    {
        public LCCostEntryUpdateDtoValidator()
        {
            Include(new LCCostEntryCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
