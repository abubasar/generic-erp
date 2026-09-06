using Application.Core.Common;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class ZoneRequestModel : BaseRequestModel<Zone>
    {
        public Guid? RegionId { get; set; }
        public override Expression<Func<Zone, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.Contains(Keyword);
            if (RegionId.HasValue) ExpressionObject = ExpressionObject.And(x => x.RegionId == RegionId.Value);
            return ExpressionObject;
        }
        public override IQueryable<Zone> IncludeParents(IQueryable<Zone> queryable)
        {
            return queryable.Include(x => x.Region);
        }
    }
}
