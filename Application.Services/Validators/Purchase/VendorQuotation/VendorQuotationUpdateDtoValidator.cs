using Application.Services.Dtos.Purchase.VendorQuotation;
using FluentValidation;

namespace Application.Services.Validators.Purchase.VendorQuotation
{
    public class VendorQuotationUpdateDtoValidator : AbstractValidator<VendorQuotationUpdateDto>
    {
        public VendorQuotationUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
