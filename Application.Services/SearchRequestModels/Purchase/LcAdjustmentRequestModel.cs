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
    public class LcAdjustmentRequestModel : BaseRequestModel<LcAdjustment>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? PurchaseInvoiceNo { get; set; }
        public Guid? CostCenterId { get; set; }
        public int? LcAdjustmentStatus { get; set; }
        public override Expression<Func<LcAdjustment, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = ExpressionObject.And(x => x.Code.ToLower().Contains(Keyword));
            if (!string.IsNullOrWhiteSpace(PurchaseInvoiceNo)) ExpressionObject = ExpressionObject.And(x => x.PurchaseInvoiceNo == PurchaseInvoiceNo);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.AdjustmentDate.Date >= FromDate.Value.ToLocal().Date && x.AdjustmentDate.Date <= ToDate.Value.ToLocal().Date);
            if (LcAdjustmentStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == LcAdjustmentStatus);
            if (CostCenterId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CostCenterId == CostCenterId.Value);
            return ExpressionObject;
        }
        public override IQueryable<LcAdjustment> IncludeParents(IQueryable<LcAdjustment> queryable)
        {
            return queryable.Include(x => x.PurchaseInvoice).Include(x => x.CostCenter).Include(x => x.LcAdjustmentDetails).ThenInclude(x => x.Account);
        }
    }
}
