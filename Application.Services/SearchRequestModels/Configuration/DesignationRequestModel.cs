using Application.Core.Common;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class DesignationRequestModel : BaseRequestModel<Designation>
    {
        public Guid? DepartmentId { get; set; }
        public override Expression<Func<Designation, bool>> GetExpression()
        {

            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Name.Contains(Keyword);
            if (DepartmentId.HasValue) ExpressionObject = ExpressionObject.And(x => x.DepartmentId == DepartmentId.Value);
            return ExpressionObject;
        }
        public override IQueryable<Designation> IncludeParents(IQueryable<Designation> queryable)
        {
            return queryable.Include(x => x.Department);
        }
    }
}
