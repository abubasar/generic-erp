using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Purchase
{
    public class PurchaseRequisitionRequestModel : BaseRequestModel<PurchaseRequisition>
    {
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public List<int> RequisitionStatusIds { get; set; } = new List<int>();

        public override Expression<Func<PurchaseRequisition, bool>> GetExpression()
        {
            ;
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = x =>
                    x.RequisitionNo.ToLower().Contains(Keyword);
            }
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.RequisitionDate.Date >= FromDate.Value.ToLocal().Date && x.RequisitionDate.Date <= ToDate.Value.ToLocal().Date);
            if (RequisitionStatusIds.Any())
            {
                ExpressionObject = ExpressionObject.And(x => RequisitionStatusIds.Contains(x.RequisitionStatus));
            }


            return ExpressionObject;
        }

        public override IQueryable<PurchaseRequisition> IncludeParents(IQueryable<PurchaseRequisition> queryable)
        {
            return queryable.Include(x => x.Department).Include(x => x.Store)
                .Include(x => x.PurchaseRequisitionDetails)
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
