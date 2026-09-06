using Application.Services.Dtos.Accounts.VoucherEntry;
using FluentValidation;

namespace Application.Services.Validators.Accounts.VoucherEntry
{
    public class VoucherEntryDetailCreationDtoValidator : AbstractValidator<VoucherEntryDetailCreationDto>
    {
        public VoucherEntryDetailCreationDtoValidator()
        {
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount can not be Empty");
        }
    }
}
