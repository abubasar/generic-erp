using Application.Core.Common;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class GenericRequestModel : BaseRequestModel<Generic>
    {
        public Guid? ProductTypeId { get; set; }

        public override Expression<Func<Generic, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.Contains(Keyword);
            if (ProductTypeId.HasValue) ExpressionObject = ExpressionObject.And(x => x.ProductTypeId == ProductTypeId.Value);
            return ExpressionObject;
        }

        public override IQueryable<Generic> IncludeParents(IQueryable<Generic> queryable)
        {
            return queryable.Include(x => x.ProductType);
        }
    }
}
