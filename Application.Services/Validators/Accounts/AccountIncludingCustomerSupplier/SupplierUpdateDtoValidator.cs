using Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier;
using FluentValidation;

namespace Application.Services.Validators.Accounts.AccountIncludingCustomerSupplier
{
    public class SupplierUpdateDtoValidator : AbstractValidator<SupplierUpdateDto>
    {
        public SupplierUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new SupplierCreationDtoValidator());
        }
    }
}
