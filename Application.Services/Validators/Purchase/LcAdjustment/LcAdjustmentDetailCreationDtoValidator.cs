using Application.Services.Dtos.Purchase.LcAdjustment;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Validators.Purchase.LcAdjustment
{
    public class LcAdjustmentDetailCreationDtoValidator : AbstractValidator<LcAdjustmentDetailCreationDto>
    {
        public LcAdjustmentDetailCreationDtoValidator()
        {
            RuleFor(x => x.AccountId).NotNull().NotEmpty().WithMessage("AccountId can not be null");
            RuleFor(x => x.AccountDescription).NotNull().NotEmpty().WithMessage("AccountDescription can not be null");
            RuleFor(x => x.PostType).NotNull().NotEmpty().WithMessage("PostType can not be null");
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount can not be Empty");
        }
    }
}
