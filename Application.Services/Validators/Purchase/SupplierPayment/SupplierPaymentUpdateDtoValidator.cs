using Application.Services.Dtos.Purchase.SupplierPayment;
using FluentValidation;

namespace Application.Services.Validators.Purchase.SupplierPayment
{
    public class SupplierPaymentUpdateDtoValidator : AbstractValidator<SupplierPaymentUpdateDto>
    {
        public SupplierPaymentUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new SupplierPaymentCreationDtoValidator());
        }
    }
}
