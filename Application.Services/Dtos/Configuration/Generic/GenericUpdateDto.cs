using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Dtos.Configuration.Generic
{
    public class GenericUpdateDto : GenericCreationDto
    {
        public Guid Id { get; set; }
    }
}
