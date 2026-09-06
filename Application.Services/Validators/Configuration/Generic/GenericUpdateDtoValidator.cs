using Application.Services.Dtos.Configuration.Generic;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Generic
{
    public class GenericUpdateDtoValidator : AbstractValidator<GenericUpdateDto>
    {
        public GenericUpdateDtoValidator()
        {
            Include(new GenericCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
