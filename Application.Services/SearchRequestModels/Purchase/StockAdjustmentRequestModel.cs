using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Purchase
{
    public class StockAdjustmentRequestModel : BaseRequestModel<StockAdjustment>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Code { get; set; }
        public Guid? StoreId { get; set; }
        public int? StockAdjustmentStatus { get; set; }
        public override Expression<Func<StockAdjustment, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Code.ToLower().Contains(Keyword);
            if (!string.IsNullOrWhiteSpace(Code)) ExpressionObject = ExpressionObject.And(x => x.Code.Contains(Code));
            if (StoreId.HasValue) ExpressionObject = ExpressionObject.And(x => x.StoreId == StoreId);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.AdjustmentDate.Date >= FromDate.Value.ToLocal().Date && x.AdjustmentDate.Date <= ToDate.Value.ToLocal().Date);
            if (StockAdjustmentStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == StockAdjustmentStatus);
            return ExpressionObject;
        }
        public override IQueryable<StockAdjustment> IncludeParents(IQueryable<StockAdjustment> queryable)
        {
            return queryable.Include(x => x.Store).Include(x => x.StockAdjustmentDetails).ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
