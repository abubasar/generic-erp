using Application.Services.Dtos.Purchase.GoodsReceiveNote;
using FluentValidation;

namespace Application.Services.Validators.Purchase.GoodsReceiveNote
{
    public class GoodsReceiveNoteDetailCreationDtoValidator : AbstractValidator<GoodsReceiveNoteDetailCreationDto>
    {
        public GoodsReceiveNoteDetailCreationDtoValidator()
        {
            RuleFor(x => x.ProductId).NotNull().NotEmpty().WithMessage("Product Id can not be Empty");
            RuleFor(x => x.Grnquantity).NotNull().NotEmpty().WithMessage("GRN Quantity is Required");
            RuleFor(x => x.Poquantity).NotNull().NotEmpty().WithMessage("PO Quantity is Required");
            RuleFor(x => x.RejectedQuantity).NotNull().NotEmpty().WithMessage("Rejected Quantity is Required");
            RuleFor(x => x.Rate).NotNull().NotEmpty().WithMessage("Rate is Required");
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount is Required");
        }
    }
}
