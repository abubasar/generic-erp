using Application.Services.Dtos.Accounts.VoucherEntry;
using FluentValidation;

namespace Application.Services.Validators.Accounts.VoucherEntry
{
    public class VoucherEntryUpdateDtoValidator : AbstractValidator<VoucherEntryUpdateDto>
    {
        public VoucherEntryUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new VoucherEntryCreationDtoValidator());
        }
    }
}
