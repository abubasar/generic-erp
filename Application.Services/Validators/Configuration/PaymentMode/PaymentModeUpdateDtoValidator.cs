using Application.Services.Dtos.Configuration.PaymentMode;
using FluentValidation;

namespace Application.Services.Validators.Configuration.PaymentMode
{
    public class PaymentModeUpdateDtoValidator : AbstractValidator<PaymentModeUpdateDto>
    {
        public PaymentModeUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new PaymentModeCreationDtoValidator());
        }
    }
}
