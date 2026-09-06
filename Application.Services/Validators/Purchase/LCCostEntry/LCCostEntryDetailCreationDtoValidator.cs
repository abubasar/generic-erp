using Application.Services.Dtos.Purchase.LCCostEntry;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Validators.Purchase.LCCostEntry
{
    public class LCCostEntryDetailCreationDtoValidator : AbstractValidator<LCCostEntryDetailCreationDto>
    {
        public LCCostEntryDetailCreationDtoValidator()
        {
            RuleFor(x => x.DebitAccountId).NotNull().NotEmpty().WithMessage("DebitAccountId can not be null");
            RuleFor(x => x.CreditAccountId).NotNull().NotEmpty().WithMessage("CreditAccountId can not be null");
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount can not be Empty");
        }
    }
}
