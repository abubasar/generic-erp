using Application.Services.Dtos.Purchase.PurchaseReturn;
using FluentValidation;

namespace Application.Services.Validators.Purchase.PurchaseReturn
{
    public class PurchaseReturnCreationDtoValidator : AbstractValidator<PurchaseReturnCreationDto>
    {
        public PurchaseReturnCreationDtoValidator()
        {
            RuleFor(x => x.PurchaseReturnDate).NotNull().NotEmpty().WithMessage("PurchaseReturnDate is Required");
        }
    }
}
