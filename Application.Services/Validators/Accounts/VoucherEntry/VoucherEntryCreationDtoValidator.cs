using Application.Services.Dtos.Accounts.VoucherEntry;
using FluentValidation;

namespace Application.Services.Validators.Accounts.VoucherEntry
{
    public class VoucherEntryCreationDtoValidator : AbstractValidator<VoucherEntryCreationDto>
    {
        public VoucherEntryCreationDtoValidator()
        {
            RuleFor(x => x.VoucherDate).NotNull().NotEmpty().WithMessage("VoucherDate can not be Empty");
        }
    }
}
