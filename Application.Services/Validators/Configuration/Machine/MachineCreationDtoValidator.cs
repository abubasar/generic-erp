using Application.Services.Dtos.Configuration.Machine;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Machine
{
    public class MachineCreationDtoValidator : AbstractValidator<MachineCreationDto>
    {
        public MachineCreationDtoValidator() 
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Machine Name is Required");
        }
    }
}
