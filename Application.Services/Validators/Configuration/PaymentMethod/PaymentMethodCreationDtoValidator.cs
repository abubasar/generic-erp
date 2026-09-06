using Application.Services.Dtos.Configuration.PaymentMethod;
using FluentValidation;

namespace Application.Services.Validators.Configuration.PaymentMethod
{
    public class PaymentMethodCreationDtoValidator : AbstractValidator<PaymentMethodCreationDto>
    {
        public PaymentMethodCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("PaymentMethod Name is Required");
        }
    }
}
