using Application.Services.Dtos.Purchase.GoodsReceiveNote;
using FluentValidation;

namespace Application.Services.Validators.Purchase.GoodsReceiveNote
{
    public class GoodsReceiveNoteCreationDtoValidator : AbstractValidator<GoodsReceiveNoteCreationDto>
    {
        public GoodsReceiveNoteCreationDtoValidator()
        {
            RuleFor(x => x.SupplierId).NotNull().NotEmpty().WithMessage("SupplierId can not be Empty");
        }
    }
}
