using Application.Services.Dtos.Configuration.Territory;
using Application.Services.Validators.Configuration.Zone;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Validators.Configuration.Territory
{
    public class TerritoryUpdateDtoValidator : AbstractValidator<TerritoryUpdateDto>
    {
        public TerritoryUpdateDtoValidator()
        {
            Include(new TerritoryCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
