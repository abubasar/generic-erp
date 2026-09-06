using Application.Core.Common;
using Application.Core.Entities;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class MeasurementUnitRequestModel : BaseRequestModel<MeasurementUnit>
    {
        public override Expression<Func<MeasurementUnit, bool>> GetExpression()
        {

            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.Contains(Keyword);
            return ExpressionObject;
        }
    }
}
