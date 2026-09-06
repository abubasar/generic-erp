using Application.Services.Dtos.Accounts.JournalEntry;
using FluentValidation;

namespace Application.Services.Validators.Accounts.JournalEntry
{
    public class JournalEntryCreationDtoValidator : AbstractValidator<JournalEntryCreationDto>
    {
        public JournalEntryCreationDtoValidator()
        {
            RuleFor(x => x.VoucherDate).NotNull().NotEmpty().WithMessage("VoucherDate can not be Empty");
        }
    }
}
