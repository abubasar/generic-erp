using Application.Services.Dtos.Configuration.Generic;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Generic
{
    public class GenericCreationDtoValidator : AbstractValidator<GenericCreationDto>
    {
        public GenericCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Generic Name is Required");
            RuleFor(x => x.ProductTypeId).NotNull().NotEmpty().WithMessage("ProductTypeId Name is Required");
        }
    }
}
