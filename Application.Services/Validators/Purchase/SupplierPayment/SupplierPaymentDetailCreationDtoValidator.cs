using Application.Services.Dtos.Purchase.SupplierPayment;
using FluentValidation;

namespace Application.Services.Validators.Purchase.SupplierPayment
{
    public class SupplierPaymentDetailCreationDtoValidator : AbstractValidator<SupplierPaymentDetailCreationDto>
    {
        public SupplierPaymentDetailCreationDtoValidator()
        {
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount can not be Empty");
        }
    }
}
