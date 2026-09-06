using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Sale
{
    public class DeliveryNoteRequestModel : BaseRequestModel<DeliveryNote>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? DeliveryNoteNo { get; set; }
        public string? SaleOrderNo { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? StoreId { get; set; }
        public int? DeliveryNoteStatus { get; set; }
        public List<int> DeliveryNoteStatuses { get; set; } = new();
        public override Expression<Func<DeliveryNote, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x =>
                    x.DeliveryNoteNo.ToLower().Contains(Keyword)
                    || x.SaleOrderNo.ToLower().Contains(Keyword)
                    || x.ReferenceNo.ToLower().Contains(Keyword));
            }
            if (CustomerId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CustomerId == CustomerId);
            if (StoreId.HasValue) ExpressionObject = ExpressionObject.And(x => x.StoreId == StoreId);
            if (!string.IsNullOrWhiteSpace(DeliveryNoteNo))
                ExpressionObject = x => x.DeliveryNoteNo == DeliveryNoteNo;
            if (!string.IsNullOrWhiteSpace(SaleOrderNo))
                ExpressionObject = x => x.SaleOrderNo == SaleOrderNo;
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.DeliveryDate.Date >= FromDate.Value.ToLocal().Date && x.DeliveryDate.Date <= ToDate.Value.ToLocal().Date);
            if (DeliveryNoteStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == DeliveryNoteStatus);
            if (DeliveryNoteStatuses.Any()) ExpressionObject = ExpressionObject.And(x => DeliveryNoteStatuses.Contains(x.Status));
            return ExpressionObject;
        }
        public override IQueryable<DeliveryNote> IncludeParents(IQueryable<DeliveryNote> queryable)
        {
            return queryable.Include(x => x.Customer).Include(x => x.Store)
                .Include(blog => blog.DeliveryNoteDetails)
                .ThenInclude(post => post.Product).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
