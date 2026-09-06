using Application.Services.Dtos.Configuration.PackSize;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Validators.Configuration.PackSize
{
    public class PackSizeUpdateDtoValidator : AbstractValidator<PackSizeUpdateDto>
    {
        public PackSizeUpdateDtoValidator()
        {
            Include(new PackSizeCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
