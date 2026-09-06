using Application.Core.Common;
using Application.Core.Entities;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class CostCenterRequestModel : BaseRequestModel<CostCenter>
    {
        public override Expression<Func<CostCenter, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.Contains(Keyword);
            return ExpressionObject;
        }
    }
}
