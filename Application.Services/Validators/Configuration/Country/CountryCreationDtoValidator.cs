using Application.Services.Dtos.Configuration.Country;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Country
{
    public class CountryCreationDtoValidator : AbstractValidator<CountryCreationDto>
    {
        public CountryCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Country Name is Required");
        }
    }
}
