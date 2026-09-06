using Application.Core.Common;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class EmployeeRequestModel : BaseRequestModel<Employee>
    {
        public Guid? DepartmentId { get; set; }
        public Guid? DesignationId { get; set; }
        public Guid? JobLocationId { get; set; }
        public string? EmployeeIdNo { get; set; }
        public override Expression<Func<Employee, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.FirstName.Contains(Keyword) || x.LastName.Contains(Keyword);
            if (!string.IsNullOrWhiteSpace(EmployeeIdNo)) ExpressionObject = ExpressionObject.And(x => x.EmployeeIdNo == EmployeeIdNo);
            if (JobLocationId.HasValue) ExpressionObject = ExpressionObject.And(x => x.JobLocationId == JobLocationId.Value);
            if (DepartmentId.HasValue) ExpressionObject = ExpressionObject.And(x => x.DepartmentId == DepartmentId.Value);
            if (DesignationId.HasValue) ExpressionObject = ExpressionObject.And(x => x.DesignationId == DesignationId.Value);
            return ExpressionObject;
        }
        public override IQueryable<Employee> IncludeParents(IQueryable<Employee> queryable)
        {
            return queryable.Include(x => x.JobLocation).Include(x => x.Department).Include(x => x.Designation);
        }
    }
}
