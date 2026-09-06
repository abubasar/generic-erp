using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Purchase
{
    public class VendorQuotationRequestModel : BaseRequestModel<VendorQuotation>
    {
        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
        public string? RequisitionNo { get; set; }
        public int? VendorQuotationStatus { get; set; }
        public override Expression<Func<VendorQuotation, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(RequisitionNo))
                ExpressionObject = x => x.RequisitionNo == RequisitionNo;
            if (VendorQuotationStatus.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.Status == VendorQuotationStatus.Value);
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x =>
                    x.QuotationNo.ToLower().Contains(Keyword) || x.RequisitionNo.ToLower().Contains(Keyword));
            }
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.CreatedOn.Date >= FromDate.Value.ToLocal().Date && x.CreatedOn.Date <= ToDate.Value.ToLocal().Date);

            return ExpressionObject;
        }
        public override IQueryable<VendorQuotation> IncludeParents(IQueryable<VendorQuotation> queryable)
        {
            return queryable.Include(x => x.Supplier)
                .Include(x => x.VendorQuotationDetails.OrderBy(x => x.Product.Name))
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
