using Application.Services.Dtos.Purchase.GoodsReceiveNote;
using FluentValidation;

namespace Application.Services.Validators.Purchase.GoodsReceiveNote
{
    public class GoodsReceiveNoteDetailUpdateDtoValidator : AbstractValidator<GoodsReceiveNoteDetailUpdateDto>
    {
        public GoodsReceiveNoteDetailUpdateDtoValidator()
        {
            Include(new GoodsReceiveNoteDetailCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
