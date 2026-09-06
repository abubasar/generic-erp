using Application.Services.Dtos.Auth;
using FluentValidation;

namespace Application.Services.Validators.Auth
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Username).MinimumLength(3).WithMessage("mIN LENGRH 2").NotEmpty().WithMessage("Username can not be Empty");
            RuleFor(x => x.Password).MinimumLength(3).WithMessage("mIN LENGRH 2").NotEmpty().WithMessage("Password can not be Empty");
        }
    }
}
