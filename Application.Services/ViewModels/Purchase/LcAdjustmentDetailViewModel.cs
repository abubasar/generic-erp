using Application.Core.Entities;
using Application.Services.ViewModels.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.ViewModels.Purchase
{
    public class LcAdjustmentDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid? LcAdjustmentId { get; set; }
        public Guid AccountId { get; set; }
        public string? AccountDescription { get; set; }
        public int PostType { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual AccountViewModel? Account { get; set; }
    }
}
