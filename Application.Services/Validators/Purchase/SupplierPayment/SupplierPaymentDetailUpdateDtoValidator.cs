using Application.Services.Dtos.Purchase.SupplierPayment;
using FluentValidation;

namespace Application.Services.Validators.Purchase.SupplierPayment
{
    public class SupplierPaymentDetailUpdateDtoValidator : AbstractValidator<SupplierPaymentDetailUpdateDto>
    {
        public SupplierPaymentDetailUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new SupplierPaymentDetailCreationDtoValidator());
        }
    }
}
