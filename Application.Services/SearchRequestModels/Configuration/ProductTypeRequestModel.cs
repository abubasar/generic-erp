using Application.Core.Common;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class ProductTypeRequestModel : BaseRequestModel<ProductType>
    {
        public Guid? InventoryTypeId { get; set; }
        public override Expression<Func<ProductType, bool>> GetExpression()
        {

            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.Contains(Keyword);
            if (InventoryTypeId.HasValue)
            {
                ExpressionObject = ExpressionObject.And(x => x.InventoryTypeId == InventoryTypeId);
            }
            return ExpressionObject;
        }

        public override IQueryable<ProductType> IncludeParents(IQueryable<ProductType> queryable)
        {
            return queryable.Include(x => x.InventoryType);
        }
    }
}
