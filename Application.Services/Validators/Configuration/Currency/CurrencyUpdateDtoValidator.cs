using Application.Services.Dtos.Configuration.Currency;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Currency
{
    public class CurrencyUpdateDtoValidator : AbstractValidator<CurrencyUpdateDto>
    {
        public CurrencyUpdateDtoValidator()
        {
            Include(new CurrencyCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
