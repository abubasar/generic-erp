using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.SearchRequestModels.Purchase
{
    public class LCCostEntryRequestModel : BaseRequestModel<LccostEntry>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Ponumber { get; set; }
        public string? LcNumber { get; set; }
        public Guid? CostCenterId { get; set; }
        public int? LCCostEntryStatus { get; set; }
        public override Expression<Func<LccostEntry, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = ExpressionObject.And(x => x.Code.ToLower().Contains(Keyword));
            if (!string.IsNullOrWhiteSpace(Ponumber)) ExpressionObject = ExpressionObject.And(x => x.Ponumber == Ponumber);
            if (!string.IsNullOrWhiteSpace(LcNumber)) ExpressionObject = ExpressionObject.And(x => x.LcNumber == LcNumber);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.EntryDate.Date >= FromDate.Value.ToLocal().Date && x.EntryDate.Date <= ToDate.Value.ToLocal().Date);
            if (LCCostEntryStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == LCCostEntryStatus);
            if (CostCenterId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CostCenterId == CostCenterId.Value);
            return ExpressionObject;
        }
        public override IQueryable<LccostEntry> IncludeParents(IQueryable<LccostEntry> queryable)
        {
            return queryable.Include(x => x.PurchaseOrder).Include(x => x.CostCenter).Include(x => x.LccostEntryDetails).ThenInclude(x => x.DebitAccount).Include(x => x.LccostEntryDetails).ThenInclude(x => x.CreditAccount);
        }
    }
}
