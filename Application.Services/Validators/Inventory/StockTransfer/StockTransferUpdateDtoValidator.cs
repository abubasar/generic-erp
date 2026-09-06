using Application.Services.Dtos.Inventory.StockTransfer;
using FluentValidation;

namespace Application.Services.Validators.Inventory.StockTransfer
{
    public class StockTransferUpdateDtoValidator : AbstractValidator<StockTransferUpdateDto>
    {
        public StockTransferUpdateDtoValidator() 
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new StockTransferCreationDtoValidator());
        }
    }
}
