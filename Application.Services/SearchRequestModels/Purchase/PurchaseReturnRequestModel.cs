using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Purchase
{
    public class PurchaseReturnRequestModel : BaseRequestModel<PurchaseReturn>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? PurchaseReturnNo { get; set; }
        public Guid? SupplierId { get; set; }
        public int? PurchaseReturnStatus { get; set; }
        public List<int> PurchaseReturnStatuses { get; set; } = new();
        public override Expression<Func<PurchaseReturn, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x =>
                    x.PurchaseReturnNo.ToLower().Contains(Keyword));
            }
            if (SupplierId.HasValue) ExpressionObject = ExpressionObject.And(x => x.SupplierId == SupplierId);
            if (!string.IsNullOrWhiteSpace(PurchaseReturnNo))
                ExpressionObject = x => x.PurchaseReturnNo == PurchaseReturnNo;
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.PurchaseReturnDate.Date >= FromDate.Value.ToLocal().Date && x.PurchaseReturnDate.Date <= ToDate.Value.ToLocal().Date);
            if (PurchaseReturnStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == PurchaseReturnStatus);
            if (PurchaseReturnStatuses.Any()) ExpressionObject = ExpressionObject.And(x => PurchaseReturnStatuses.Contains(x.Status));
            return ExpressionObject;
        }
        public override IQueryable<PurchaseReturn> IncludeParents(IQueryable<PurchaseReturn> queryable)
        {
            return queryable.Include(x => x.Supplier).Include(x => x.Store)
                .Include(blog => blog.PurchaseReturnDetails)
                .ThenInclude(post => post.Product).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
