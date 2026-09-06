using Application.Core.Common;
using Application.Core.Entities;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class EmailAccountRequestModel : BaseRequestModel<EmailAccount>
    {
        public override Expression<Func<EmailAccount, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.DisplayName.Contains(Keyword) || x.Email.Contains(Keyword) || x.Username.Contains(Keyword);
            return ExpressionObject;
        }
    }
}
