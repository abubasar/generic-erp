using Application.Services.Dtos.Configuration.Category;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Validators.Configuration.Category
{
    public class CategoryCreationDtoValidator : AbstractValidator<CategoryCreationDto>
    {
        public CategoryCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Category Name is Required");
        }
    }
}
