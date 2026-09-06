using Application.Core.Common;
using Application.Core.Entities;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class FundTransferTransactionTypeRequestModel : BaseRequestModel<FundTransferTransactionType>
    {
        public override Expression<Func<FundTransferTransactionType, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.Contains(Keyword);
            return ExpressionObject;
        }
    }
}
