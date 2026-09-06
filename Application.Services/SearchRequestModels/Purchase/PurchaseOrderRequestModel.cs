using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Purchase
{
    public class PurchaseOrderRequestModel : BaseRequestModel<PurchaseOrder>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? SupplierId { get; set; }
        public string? Ponumber { get; set; }
        public bool IsImportPurchase { get; set; }
        public string? LcNumber { get; set; }
        public List<int> PurchaseOrderStatuses { get; set; } = new();
        public int? PurchaseOrderStatus { get; set; }
        public override Expression<Func<PurchaseOrder, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = x =>
                    x.Ponumber.ToLower().Contains(Keyword) || x.QuotationNo.ToLower().Contains(Keyword) || x.RequisitionNo.ToLower().Contains(Keyword);
            }
            if (!string.IsNullOrWhiteSpace(Ponumber)) ExpressionObject = ExpressionObject.And(x => x.Ponumber == Ponumber);
            if (!string.IsNullOrWhiteSpace(LcNumber)) ExpressionObject = ExpressionObject.And(x => x.LcNumber == LcNumber);
            if (SupplierId.HasValue) ExpressionObject = ExpressionObject.And(x => x.SupplierId == SupplierId);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.Podate.Date >= FromDate.Value.ToLocal().Date && x.Podate.Date <= ToDate.Value.ToLocal().Date);
            if (IsImportPurchase) ExpressionObject = ExpressionObject.And(x => x.IsImportPurchase == IsImportPurchase);
            if (PurchaseOrderStatuses.Any()) ExpressionObject = ExpressionObject.And(x => PurchaseOrderStatuses.Contains(x.Status));
            if (PurchaseOrderStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == PurchaseOrderStatus);
            return ExpressionObject;
        }
        public override IQueryable<PurchaseOrder> IncludeParents(IQueryable<PurchaseOrder> queryable)
        {
            return queryable.Include(x => x.Supplier).Include(x => x.Store).Include(x => x.DeliveryPlace)
                .Include(x => x.PurchaseOrderDetails)
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
