using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Sale
{
    public class SaleOrderRequestModel : BaseRequestModel<SaleOrder>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public Guid? CustomerTerritoryId { get; set; }
        public Guid? StoreId { get; set; }
        public string? SaleOrderNo { get; set; }
        public string? QuotationNo { get; set; }
        public List<int> SaleOrderStatuses { get; set; } = new();
        public int? SaleOrderStatus { get; set; }
        public override Expression<Func<SaleOrder, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x =>
                    x.SaleOrderNo.ToLower().Contains(Keyword) || x.ReferenceNo.ToLower().Contains(Keyword));
            }
            if (CustomerId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CustomerId == CustomerId);
            if (CustomerMarketingOfficerId.HasValue) ExpressionObject = ExpressionObject.And(x => x.Customer.CustomerMarketingOfficerId == CustomerMarketingOfficerId.Value);
            if (CustomerTerritoryId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CustomerTerritoryId == CustomerTerritoryId.Value);
            if (StoreId.HasValue) ExpressionObject = ExpressionObject.And(x => x.StoreId == StoreId);
            if (!string.IsNullOrWhiteSpace(SaleOrderNo))
                ExpressionObject = x => x.SaleOrderNo == SaleOrderNo;
            if (!string.IsNullOrWhiteSpace(QuotationNo))
                ExpressionObject = x => x.QuotationNo == QuotationNo;
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.OrderDate.Date >= FromDate.Value.ToLocal().Date && x.OrderDate.Date <= ToDate.Value.ToLocal().Date);
            if (SaleOrderStatuses.Any()) ExpressionObject = ExpressionObject.And(x => SaleOrderStatuses.Contains(x.Status));
            if (SaleOrderStatus.HasValue) ExpressionObject = ExpressionObject.And(x => SaleOrderStatus.Value == x.Status);
            return ExpressionObject;
        }
        public override IQueryable<SaleOrder> IncludeParents(IQueryable<SaleOrder> queryable)
        {
            return queryable.Include(x => x.Customer).Include(x => x.Store).Include(x => x.CustomerTerritory)
                .Include(blog => blog.SaleOrderDetails)
                .ThenInclude(post => post.Product).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
