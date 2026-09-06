using Application.Services.Dtos.Purchase.GoodsReceiveNote;
using FluentValidation;

namespace Application.Services.Validators.Purchase.GoodsReceiveNote
{
    public class GoodsReceiveNoteUpdateDtoValidator : AbstractValidator<GoodsReceiveNoteUpdateDto>
    {
        public GoodsReceiveNoteUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
