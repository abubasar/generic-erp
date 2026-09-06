using Application.Services.Dtos.Inventory.StockTransfer;
using FluentValidation;

namespace Application.Services.Validators.Inventory.StockTransfer
{
    public class StockTransferCreationDtoValidator : AbstractValidator<StockTransferCreationDto>
    {
        public StockTransferCreationDtoValidator() 
        {
            RuleFor(x => x.TransferDate).NotNull().NotEmpty().WithMessage("TransferDate can not be Empty");
            RuleFor(x => x.SourceId).NotNull().NotEmpty().WithMessage("SourceId can not be Empty");
            RuleFor(x => x.DestinationId).NotNull().NotEmpty().WithMessage("DestinationId can not be Empty");
        }
    }
}
