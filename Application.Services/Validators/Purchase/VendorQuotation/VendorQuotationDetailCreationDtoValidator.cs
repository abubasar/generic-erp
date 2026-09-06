using Application.Services.Dtos.Purchase.VendorQuotation;
using FluentValidation;

namespace Application.Services.Validators.Purchase.VendorQuotation
{
    public class VendorQuotationDetailCreationDtoValidator : AbstractValidator<VendorQuotationDetailCreationDto>
    {
        public VendorQuotationDetailCreationDtoValidator()
        {
            RuleFor(x => x.ProductId).NotNull().NotEmpty().WithMessage("Product Id can not be Empty");
            RuleFor(x => x.Quantity).NotNull().NotEmpty().WithMessage("Quantity is Required");
            RuleFor(x => x.Rate).NotNull().NotEmpty().WithMessage("Rate is Required");
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount is Required");
        }
    }
}
