using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Dtos.Configuration.Territory
{
    public class TerritoryCreationDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid AreaId { get; set; }
    }
}
