using Application.Core.Common;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Accounts
{
    public class SupplierRequestModel : BaseRequestModel<Account>
    {
        public Guid? ProductTypeId { get; set; }
        public override Expression<Func<Account, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.Contains(Keyword) || x.Code.Contains(Keyword);
            //  if(ProductTypeId.HasValue) ExpressionObject=ExpressionObject.And(x=>x.SupplierProductTypeId==ProductTypeId);
            return ExpressionObject;
        }
        public override IQueryable<Account> IncludeParents(IQueryable<Account> queryable)
        {
            return queryable.Include(x => x.BankAccounts);
        }
    }
}