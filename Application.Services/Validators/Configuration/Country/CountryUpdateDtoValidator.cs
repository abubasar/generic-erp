using Application.Services.Dtos.Configuration.Country;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Country
{
    public class CountryUpdateDtoValidator : AbstractValidator<CountryUpdateDto>
    {
        public CountryUpdateDtoValidator()
        {
            Include(new CountryCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
