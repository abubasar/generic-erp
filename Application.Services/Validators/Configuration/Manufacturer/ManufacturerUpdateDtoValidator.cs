using Application.Services.Dtos.Configuration.Manufacturer;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Validators.Configuration.Manufacturer
{
    public class ManufacturerUpdateDtoValidator : AbstractValidator<ManufacturerUpdateDto>
    {
        public ManufacturerUpdateDtoValidator()
        {
            Include(new ManufacturerCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
