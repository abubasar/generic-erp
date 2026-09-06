using Application.Services.Dtos.Inventory.StockTransfer;
using FluentValidation;

namespace Application.Services.Validators.Inventory.StockTransfer
{
    public class StockTransferDetailCreationDtoValidator : AbstractValidator<StockTransferDetailCreationDto>
    {
        public StockTransferDetailCreationDtoValidator()
        {
            RuleFor(x => x.ProductId).NotNull().NotEmpty().WithMessage("ProductId can not be Empty");
            RuleFor(x => x.TransferBagQuantity).NotNull().NotEmpty().WithMessage("TransferBagQuantity can not be Empty");
            RuleFor(x => x.TransferQuantity).NotNull().NotEmpty().WithMessage("TransferQuantity can not be Empty");
        }
    }
}
