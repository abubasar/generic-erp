using Application.Services.Dtos.Inventory.StockTransfer;
using FluentValidation;

namespace Application.Services.Validators.Inventory.StockTransfer
{
    public class StockTransferDetailUpdateDtoValidator : AbstractValidator<StockTransferDetailUpdateDto>
    {
        public StockTransferDetailUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new StockTransferDetailCreationDtoValidator());
        }
    }
}
