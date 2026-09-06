using Application.Core.Common;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class ProductCostSetupRequestModel : BaseRequestModel<ProductCostSetup>
    {
        public override Expression<Func<ProductCostSetup, bool>> GetExpression()
        {
            return ExpressionObject;
        }

        public override IQueryable<ProductCostSetup> IncludeParents(IQueryable<ProductCostSetup> queryable)
        {
            return queryable.Include(x => x.Product);
        }
    }
}
