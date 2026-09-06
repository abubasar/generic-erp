using Application.Services.Dtos.Purchase.LCCostEntry;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Validators.Purchase.LCCostEntry
{
    public class LCCostEntryCreationDtoValidator : AbstractValidator<LCCostEntryCreationDto>
    {
        public LCCostEntryCreationDtoValidator()
        {
            RuleFor(x => x.PurchaseOrderId).NotNull().NotEmpty().WithMessage("PurchaseOrderId can not be Empty");
            RuleFor(x => x.EntryDate).NotNull().NotEmpty().WithMessage("EntryDate can not be Empty");
            RuleFor(x => x.Ponumber).NotNull().NotEmpty().WithMessage("Ponumber can not be Empty");
            RuleFor(x => x.LcNumber).NotNull().NotEmpty().WithMessage("LcNumber can not be Empty");
            RuleFor(x => x.Total).NotNull().NotEmpty().WithMessage("Total can not be Empty");
        }
    }
}
