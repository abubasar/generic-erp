using Application.Services.Dtos.Sale.SaleQuotation;
using FluentValidation;

namespace Application.Services.Validators.Sale.SaleQuotation
{
    public class SaleQuotationUpdateDtoValidator : AbstractValidator<SaleQuotationUpdateDto>
    {
        public SaleQuotationUpdateDtoValidator()
        {
            Include(new SaleQuotationCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
