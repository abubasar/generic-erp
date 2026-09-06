using Application.Services.Dtos.Configuration.EmailAccount;
using FluentValidation;

namespace Application.Services.Validators.Configuration.EmailAccount
{
    public class EmailAccountCreationDtoValidator : AbstractValidator<EmailAccountCreationDto>
    {
        public EmailAccountCreationDtoValidator()
        {
            RuleFor(x => x.DisplayName).NotNull().NotEmpty().WithMessage("Display Name is Required");
            RuleFor(x => x.Email).NotNull().NotEmpty().WithMessage("Email is Required");
            RuleFor(x => x.Host).NotNull().NotEmpty().WithMessage("Host is Required");
            RuleFor(x => x.Username).NotNull().NotEmpty().WithMessage("UserName is Required");
            RuleFor(x => x.Password).NotNull().NotEmpty().WithMessage("Password is Required");
            RuleFor(x => x.Port).NotNull().NotEmpty().WithMessage("Port is Required");
        }
    }
}
