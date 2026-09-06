using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Dtos.Configuration.Territory
{
    public class TerritoryUpdateDto : TerritoryCreationDto
    {
        public Guid Id { get; set; }
    }
}
