using Application.Services.Dtos.Configuration.PaymentMode;
using FluentValidation;

namespace Application.Services.Validators.Configuration.PaymentMode
{
    public class PaymentModeCreationDtoValidator : AbstractValidator<PaymentModeCreationDto>
    {
        public PaymentModeCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Name is Required");
        }
    }
}
