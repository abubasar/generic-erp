using Application.Services.Dtos.Sale.SaleQuotation;
using FluentValidation;

namespace Application.Services.Validators.Sale.SaleQuotation
{
    public class SaleQuotationDetailUpdateDtoValidator : AbstractValidator<SaleQuotationDetailUpdateDto>
    {
        public SaleQuotationDetailUpdateDtoValidator()
        {
            Include(new SaleQuotationDetailCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
