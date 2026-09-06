using Application.Services.Dtos.Configuration.Territory;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Validators.Configuration.Territory
{
    public class TerritoryCreationDtoValidator : AbstractValidator<TerritoryCreationDto>
    {
        public TerritoryCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Territory Name is Required");
            RuleFor(x => x.AreaId).NotNull().NotEmpty().WithMessage("AreaId is Required");
        }
    }
}
