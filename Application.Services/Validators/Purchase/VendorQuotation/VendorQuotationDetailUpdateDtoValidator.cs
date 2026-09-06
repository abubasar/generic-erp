using Application.Services.Dtos.Purchase.VendorQuotation;
using FluentValidation;

namespace Application.Services.Validators.Purchase.VendorQuotation
{
    public class VendorQuotationDetailUpdateDtoValidator : AbstractValidator<VendorQuotationDetailUpdateDto>
    {
        public VendorQuotationDetailUpdateDtoValidator()
        {
            Include(new VendorQuotationDetailCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
