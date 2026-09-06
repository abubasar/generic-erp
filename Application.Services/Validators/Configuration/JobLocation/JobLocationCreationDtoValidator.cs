using Application.Services.Dtos.Configuration.JobLocation;
using FluentValidation;

namespace Application.Services.Validators.Configuration.JobLocation
{
    public class JobLocationCreationDtoValidator : AbstractValidator<JobLocationCreationDto>
    {
        public JobLocationCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Job Location Name is Required");
        }
    }
}
