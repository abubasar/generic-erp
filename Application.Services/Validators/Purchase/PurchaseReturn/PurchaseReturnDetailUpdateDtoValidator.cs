using Application.Services.Dtos.Purchase.PurchaseReturn;
using FluentValidation;

namespace Application.Services.Validators.Purchase.PurchaseReturn
{
    public class PurchaseReturnDetailUpdateDtoValidator : AbstractValidator<PurchaseReturnDetailUpdateDto>
    {
        public PurchaseReturnDetailUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new PurchaseReturnDetailCreationDtoValidator());
        }
    }
}
