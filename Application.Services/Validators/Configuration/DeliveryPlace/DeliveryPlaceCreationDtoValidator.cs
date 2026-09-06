using Application.Services.Dtos.Configuration.DeliveryPlace;
using FluentValidation;

namespace Application.Services.Validators.Configuration.DeliveryPlace
{
    public class DeliveryPlaceCreationDtoValidator : AbstractValidator<DeliveryPlaceCreationDto>
    {
        public DeliveryPlaceCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("DeliveryPlace Name is Required");
        }
    }
}
