using Application.Services.Dtos.Configuration.PaymentMethod;
using FluentValidation;

namespace Application.Services.Validators.Configuration.PaymentMethod
{
    public class PaymentMethodUpdateDtoValidator : AbstractValidator<PaymentMethodUpdateDto>
    {
        public PaymentMethodUpdateDtoValidator()
        {
            Include(new PaymentMethodCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
