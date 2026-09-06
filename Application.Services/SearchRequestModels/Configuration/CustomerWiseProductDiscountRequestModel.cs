using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Configuration
{
    public class CustomerWiseProductDiscountRequestModel : BaseRequestModel<CustomerWiseProductDiscount>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? CustomerZoneId { get; set; }
        public Guid? CustomerAreaId { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public bool IsActive { get; set; }
        public int? CustomerWiseProductDiscountStatus { get; set; }
        public override Expression<Func<CustomerWiseProductDiscount, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Customer.Name.Contains(Keyword);
            if (CustomerId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CustomerId == CustomerId);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.ApplicableDate.Date >= FromDate.Value.ToLocal().Date && x.ApplicableDate.Date <= ToDate.Value.ToLocal().Date);
            if (CustomerZoneId.HasValue) ExpressionObject = ExpressionObject.And(x => x.Customer.CustomerZoneId == CustomerZoneId.Value);
            if (CustomerAreaId.HasValue) ExpressionObject = ExpressionObject.And(x => x.Customer.CustomerAreaId == CustomerAreaId.Value);
            if (CustomerMarketingOfficerId.HasValue) ExpressionObject = ExpressionObject.And(x => x.Customer.CustomerMarketingOfficerId == CustomerMarketingOfficerId.Value);
            if (IsActive || !IsActive) ExpressionObject = ExpressionObject.And(x => x.IsActive == IsActive);
            if (CustomerWiseProductDiscountStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == CustomerWiseProductDiscountStatus);
            return ExpressionObject;
        }
        public override IQueryable<CustomerWiseProductDiscount> IncludeParents(IQueryable<CustomerWiseProductDiscount> queryable)
        {
            return queryable.Include(x => x.Customer)
                .Include(blog => blog.CustomerWiseProductDiscountDetails)
                .ThenInclude(post => post.Product).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
