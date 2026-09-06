using Application.Core.Common;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Auth
{
    public class UserRequestModel : BaseRequestModel<User>
    {
        public override Expression<Func<User, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Username.Contains(Keyword)
            || x.Role.Name.Contains(Keyword);

            return ExpressionObject;

        }
        public override IQueryable<User> IncludeParents(IQueryable<User> queryable)
        {
            return queryable.Include(x => x.Role).Include(x=>x.Employee);
        }
    }
}
