using Application.Core.Common;
using Application.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class PackSizeRequestModel : BaseRequestModel<PackSize>
    {
        public override Expression<Func<PackSize, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.Contains(Keyword);
            return ExpressionObject;
        }
    }
}
