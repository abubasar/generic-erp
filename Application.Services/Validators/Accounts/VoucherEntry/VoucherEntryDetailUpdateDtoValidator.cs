using Application.Services.Dtos.Accounts.VoucherEntry;
using FluentValidation;

namespace Application.Services.Validators.Accounts.VoucherEntry
{
    public class VoucherEntryDetailUpdateDtoValidator : AbstractValidator<VoucherEntryDetailUpdateDto>
    {
        public VoucherEntryDetailUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new VoucherEntryDetailCreationDtoValidator());
        }
    }
}
