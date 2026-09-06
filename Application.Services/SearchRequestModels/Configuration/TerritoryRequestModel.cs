using Application.Core.Common;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class TerritoryRequestModel : BaseRequestModel<Territory>
    {
        public Guid? AreaId { get; set; }
        public override Expression<Func<Territory, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.Contains(Keyword);
            if (AreaId.HasValue) ExpressionObject = ExpressionObject.And(x => x.AreaId == AreaId.Value);
            return ExpressionObject;
        }
        public override IQueryable<Territory> IncludeParents(IQueryable<Territory> queryable)
        {
            return queryable.Include(x => x.Area);
        }
    }
}
