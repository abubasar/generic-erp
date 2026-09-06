using Application.Services.Dtos.Accounts.JournalEntry;
using FluentValidation;

namespace Application.Services.Validators.Accounts.JournalEntry
{
    public class JournalEntryUpdateDtoValidator : AbstractValidator<JournalEntryUpdateDto>
    {
        public JournalEntryUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new JournalEntryCreationDtoValidator());
        }
    }
}
