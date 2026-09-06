using Application.Services.Dtos.Configuration.Tenant;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Tenant
{
    public class TenantCreationDtoValidation : AbstractValidator<TenantCreationDto>
    {
        public TenantCreationDtoValidation()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Tenant Name is Required");
        }
    }
}
