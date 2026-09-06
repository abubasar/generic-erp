using Application.Core.Common;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Accounts
{
    public class AccountRequestModel : BaseRequestModel<Account>
    {
        public Guid? AccountTypeId { get; set; }
        public Guid? ParentId { get; set; }
        public override Expression<Func<Account, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.Contains(Keyword);
            if (AccountTypeId.HasValue) ExpressionObject = ExpressionObject.And(x => x.AccountTypeId == AccountTypeId);
            if (ParentId.HasValue) ExpressionObject = ExpressionObject.And(x => x.ParentId == ParentId);
            return ExpressionObject;
        }
        public override IQueryable<Account> IncludeParents(IQueryable<Account> queryable)
        {
            return queryable.Include(x => x.AccountType).Include(x => x.Parent);
        }
    }
}