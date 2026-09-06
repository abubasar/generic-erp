using Application.Services.Dtos.Sale.SaleReturn;
using FluentValidation;

namespace Application.Services.Validators.Sale.SaleReturn
{
    public class SaleReturnDetailUpdateDtoValidator : AbstractValidator<SaleReturnDetailUpdateDto>
    {
        public SaleReturnDetailUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new SaleReturnDetailCreationDtoValidator());
        }
    }
}
