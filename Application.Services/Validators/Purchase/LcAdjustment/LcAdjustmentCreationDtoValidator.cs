using Application.Services.Dtos.Purchase.LcAdjustment;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Validators.Purchase.LcAdjustment
{
    public class LcAdjustmentCreationDtoValidator : AbstractValidator<LcAdjustmentCreationDto>
    {
        public LcAdjustmentCreationDtoValidator()
        {
            RuleFor(x => x.PurchaseInvoiceId).NotNull().NotEmpty().WithMessage("PurchaseInvoiceId can not be Empty");
            RuleFor(x => x.AdjustmentDate).NotNull().NotEmpty().WithMessage("AdjustmentDate can not be Empty");
            RuleFor(x => x.PurchaseInvoiceNo).NotNull().NotEmpty().WithMessage("PurchaseInvoiceNo can not be Empty");
            RuleFor(x => x.InvoiceTotal).GreaterThanOrEqualTo(0).WithMessage("InvoiceTotal can not be less than 0");
            RuleFor(x => x.LcMarginTotal).GreaterThanOrEqualTo(0).WithMessage("LcMarginTotal can not be less than 0");
        }
    }
}
