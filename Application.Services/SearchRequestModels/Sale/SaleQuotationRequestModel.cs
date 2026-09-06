using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Sale
{
    public class SaleQuotationRequestModel : BaseRequestModel<SaleQuotation>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? QuotationNo { get; set; }
        public int? SaleQuotationStatus { get; set; }
        public override Expression<Func<SaleQuotation, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(QuotationNo))
                ExpressionObject = x => x.QuotationNo == QuotationNo;
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x =>
                    x.QuotationNo.ToLower().Contains(Keyword));
            }
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.QuotationDate.Date >= FromDate.Value.ToLocal().Date && x.QuotationDate.Date <= ToDate.Value.ToLocal().Date);
            if (SaleQuotationStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == SaleQuotationStatus);
            return ExpressionObject;
        }

        public override IQueryable<SaleQuotation> IncludeParents(IQueryable<SaleQuotation> queryable)
        {
            return queryable.Include(x => x.Customer)
                .Include(blog => blog.SaleQuotationDetails)
                .ThenInclude(post => post.Product).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
