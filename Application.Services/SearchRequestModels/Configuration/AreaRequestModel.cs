using Application.Core.Common;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class AreaRequestModel : BaseRequestModel<Area>
    {
        public Guid? ZoneId { get; set; }
        public override Expression<Func<Area, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.Contains(Keyword);
            if (ZoneId.HasValue) ExpressionObject = ExpressionObject.And(x => x.ZoneId == ZoneId.Value);
            return ExpressionObject;
        }
        public override IQueryable<Area> IncludeParents(IQueryable<Area> queryable)
        {
            return queryable.Include(x => x.Zone).ThenInclude(x => x.Region);
        }
    }
}
