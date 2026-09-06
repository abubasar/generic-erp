using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Sale
{
    public class SaleReturnRequestModel : BaseRequestModel<SaleReturn>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? StoreId { get; set; }
        public string? SaleReturnNo { get; set; }
        public int? SaleReturnStatus { get; set; }
        public override Expression<Func<SaleReturn, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x =>
                    x.SaleReturnNo.ToLower().Contains(Keyword));
            }
            if (CustomerId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CustomerId == CustomerId);
            if (StoreId.HasValue) ExpressionObject = ExpressionObject.And(x => x.StoreId == StoreId);
            if (!string.IsNullOrWhiteSpace(SaleReturnNo))
                ExpressionObject = x => x.SaleReturnNo == SaleReturnNo;
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.SaleReturnDate.Date >= FromDate.Value.ToLocal().Date && x.SaleReturnDate.Date <= ToDate.Value.ToLocal().Date);
            if (SaleReturnStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == SaleReturnStatus);
            return ExpressionObject;
        }
        public override IQueryable<SaleReturn> IncludeParents(IQueryable<SaleReturn> queryable)
        {
            return queryable.Include(x => x.Customer).Include(x => x.Store)
                .Include(blog => blog.SaleReturnDetails)
                .ThenInclude(post => post.Product).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
