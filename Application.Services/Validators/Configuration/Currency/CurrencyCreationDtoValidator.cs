using Application.Services.Dtos.Configuration.Currency;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Currency
{
    public class CurrencyCreationDtoValidator : AbstractValidator<CurrencyCreationDto>
    {
        public CurrencyCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Currency Name is Required");
        }
    }
}
