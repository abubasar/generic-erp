using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Configuration
{
    public class TerritoryViewModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid AreaId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual AreaViewModel? Area { get; set; }
    }
}
