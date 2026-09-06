using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class DiscountProductWiseRequestModel : BaseRequestModel<DiscountProductWise>
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public override Expression<Func<DiscountProductWise, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.Contains(Keyword);
            if (StartDate.HasValue && EndDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.StartDate.Date >= StartDate.Value.ToLocal().Date && x.EndDate.Date <= EndDate.Value.ToLocal().Date);
            return ExpressionObject;
        }
        public override IQueryable<DiscountProductWise> IncludeParents(IQueryable<DiscountProductWise> queryable)
        {
            return queryable.Include(blog => blog.DiscountProductWiseDetails).ThenInclude(post => post.Product).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
