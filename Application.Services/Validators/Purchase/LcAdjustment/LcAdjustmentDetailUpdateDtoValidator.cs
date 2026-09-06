using Application.Services.Dtos.Purchase.LcAdjustment;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Validators.Purchase.LcAdjustment
{
    public class LcAdjustmentDetailUpdateDtoValidator : AbstractValidator<LcAdjustmentDetailUpdateDto>
    {
        public LcAdjustmentDetailUpdateDtoValidator()
        {
            Include(new LcAdjustmentDetailCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
