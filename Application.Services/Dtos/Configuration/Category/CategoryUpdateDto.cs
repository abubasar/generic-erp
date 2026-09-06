using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Dtos.Configuration.Category
{
    public class CategoryUpdateDto : CategoryCreationDto
    {
        public Guid Id { get; set; }
    }
}
