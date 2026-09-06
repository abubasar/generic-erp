using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Purchase
{
    public class PoPriceAdjustmentAfterGrnRequestModel : BaseRequestModel<PoPriceAdjustmentAfterGrn>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Code { get; set; }
        public Guid? SupplierId { get; set; }
        public int? PoPriceAdjustmentAfterGrnStatus { get; set; }
        public List<int> PoPriceAdjustmentAfterGrnStatuses { get; set; } = new();
        public override Expression<Func<PoPriceAdjustmentAfterGrn, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x =>
                    x.Code.ToLower().Contains(Keyword));
            }
            if (SupplierId.HasValue) ExpressionObject = ExpressionObject.And(x => x.SupplierId == SupplierId);
            if (!string.IsNullOrWhiteSpace(Code))
                ExpressionObject = x => x.Code == Code;
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.AdjustmentDate.Date >= FromDate.Value.ToLocal().Date && x.AdjustmentDate.Date <= ToDate.Value.ToLocal().Date);
            if (PoPriceAdjustmentAfterGrnStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == PoPriceAdjustmentAfterGrnStatus);
            if (PoPriceAdjustmentAfterGrnStatuses.Any()) ExpressionObject = ExpressionObject.And(x => PoPriceAdjustmentAfterGrnStatuses.Contains(x.Status));
            return ExpressionObject;
        }
        public override IQueryable<PoPriceAdjustmentAfterGrn> IncludeParents(IQueryable<PoPriceAdjustmentAfterGrn> queryable)
        {
            return queryable.Include(x => x.Supplier).Include(x => x.Store)
                .Include(blog => blog.PoPriceAdjustmentAfterGrnDetails)
                .ThenInclude(post => post.Product).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
