using Application.Services.Dtos.Sale.SaleQuotation;
using FluentValidation;

namespace Application.Services.Validators.Sale.SaleQuotation
{
    public class SaleQuotationCreationDtoValidator : AbstractValidator<SaleQuotationCreationDto>
    {
        public SaleQuotationCreationDtoValidator()
        {
            RuleFor(x => x.ReferenceNo).NotNull().NotEmpty().WithMessage("Reference Number is Required");
        }
    }
}
