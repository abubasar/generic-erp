using Application.Services.Dtos.Purchase.PurchaseReturn;
using FluentValidation;

namespace Application.Services.Validators.Purchase.PurchaseReturn
{
    public class PurchaseReturnUpdateDtoValidator : AbstractValidator<PurchaseReturnUpdateDto>
    {
        public PurchaseReturnUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new PurchaseReturnCreationDtoValidator());
        }
    }
}
