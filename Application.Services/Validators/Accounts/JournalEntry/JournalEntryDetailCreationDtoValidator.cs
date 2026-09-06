using Application.Services.Dtos.Accounts.JournalEntry;
using FluentValidation;

namespace Application.Services.Validators.Accounts.JournalEntry
{
    public class JournalEntryDetailCreationDtoValidator : AbstractValidator<JournalEntryDetailCreationDto>
    {
        public JournalEntryDetailCreationDtoValidator()
        {
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount can not be Empty");
        }
    }
}
