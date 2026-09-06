using Application.Services.Dtos.Sale.SaleReturn;
using FluentValidation;

namespace Application.Services.Validators.Sale.SaleReturn
{
    public class SaleReturnCreationDtoValidator : AbstractValidator<SaleReturnCreationDto>
    {
        public SaleReturnCreationDtoValidator()
        {
            RuleFor(x => x.SaleReturnDate).NotNull().NotEmpty().WithMessage("SaleReturnDate is Required");
        }
    }
}
