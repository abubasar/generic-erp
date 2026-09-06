using Application.Core.Common;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class StoreRequestModel : BaseRequestModel<Store>
    {
        public Guid? InventoryTypeId { get; set; }
        public override Expression<Func<Store, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.Contains(Keyword);
            if (InventoryTypeId.HasValue) ExpressionObject = ExpressionObject.And(x => x.InventoryTypeId == InventoryTypeId.Value);
            return ExpressionObject;
        }

        public override IQueryable<Store> IncludeParents(IQueryable<Store> queryable)
        {
            return queryable.Include(x => x.InventoryType);
        }
    }
}
