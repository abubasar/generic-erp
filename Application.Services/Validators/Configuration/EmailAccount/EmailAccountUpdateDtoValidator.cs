using Application.Services.Dtos.Configuration.EmailAccount;
using FluentValidation;

namespace Application.Services.Validators.Configuration.EmailAccount
{
    public class EmailAccountUpdateDtoValidator : AbstractValidator<EmailAccountUpdateDto>
    {
        public EmailAccountUpdateDtoValidator()
        {
            Include(new EmailAccountCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
