using Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier;
using FluentValidation;

namespace Application.Services.Validators.Accounts.AccountIncludingCustomerSupplier
{
    public class SupplierCreationDtoValidator : AbstractValidator<SupplierCreationDto>
    {
        public SupplierCreationDtoValidator()
        {
            RuleFor(x => x.ContactNo).NotNull().NotEmpty().WithMessage("ContactNo can not be Empty").Matches("^01[3-9]\\d{8}$").WithMessage("Invalid contact number. It should be an 11-digit number starting with 01 and the third digit cannot be 0, 1, or 2.");
            RuleFor(x => x.ContactPersonContactNo).NotNull().NotEmpty().WithMessage("ContactPersonContactNo can not be Empty").Matches("^01[3-9]\\d{8}$").WithMessage("Invalid contact number. It should be an 11-digit number starting with 01 and the third digit cannot be 0, 1, or 2.");
        }
    }
}
