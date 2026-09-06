using Application.Services.Dtos.Configuration.PackSize;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Validators.Configuration.PackSize
{
    public class PackSizeCreationDtoValidator : AbstractValidator<PackSizeCreationDto>
    {
        public PackSizeCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Pack Size Name is Required");
        }
    }
}
