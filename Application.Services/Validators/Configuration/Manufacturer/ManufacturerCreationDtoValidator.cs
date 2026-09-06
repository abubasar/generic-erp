using Application.Services.Dtos.Configuration.Manufacturer;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Validators.Configuration.Manufacturer
{
    public class ManufacturerCreationDtoValidator : AbstractValidator<ManufacturerCreationDto>
    {
        public ManufacturerCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Manufacturer Name is Required");
        }
    }
}
