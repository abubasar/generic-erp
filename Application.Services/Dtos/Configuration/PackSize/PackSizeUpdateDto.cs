using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Dtos.Configuration.PackSize
{
    public class PackSizeUpdateDto : PackSizeCreationDto
    {
        public Guid Id { get; set; }
    }
}
