using Application.Core.Common;
using Application.Core.Entities;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Auth
{
    public class RoleRequestModel : BaseRequestModel<Role>
    {
        public override Expression<Func<Role, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.Contains(Keyword);
            return ExpressionObject;
        }
    }
}
