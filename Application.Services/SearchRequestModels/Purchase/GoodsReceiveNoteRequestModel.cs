using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Purchase
{
    public class GoodsReceiveNoteRequestModel : BaseRequestModel<GoodsReceiveNote>
    {
        /* ReportType is being used to indentify Pdf or excel in report design*/
        public int ReportType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? SupplierId { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? ProductId { get; set; }
        public string? Ponumber { get; set; }
        public string? Grnno { get; set; }
        public bool IsImportPurchase { get; set; }
        public string? LcNumber { get; set; }
        public int? GRNStatus { get; set; }
        public List<int> GRNStatuses { get; set; } = new();
        public override Expression<Func<GoodsReceiveNote, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = x =>
                   x.Grnno.ToLower().Contains(Keyword) || x.Ponumber.ToLower().Contains(Keyword) || x.ChallanNo.ToLower().Contains(Keyword);
            }
            if (!string.IsNullOrWhiteSpace(Ponumber)) ExpressionObject = ExpressionObject.And(x => x.Ponumber.Contains(Ponumber));
            if (!string.IsNullOrWhiteSpace(LcNumber)) ExpressionObject = ExpressionObject.And(x => x.LcNumber == LcNumber);
            if (!string.IsNullOrWhiteSpace(Grnno)) ExpressionObject = ExpressionObject.And(x => x.Grnno.Contains(Grnno));
            if (SupplierId.HasValue) ExpressionObject = ExpressionObject.And(x => x.SupplierId == SupplierId);
            if (StoreId.HasValue) ExpressionObject = ExpressionObject.And(x => x.StoreId == StoreId);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.Grndate.Date >= FromDate.Value.ToLocal().Date && x.Grndate.Date <= ToDate.Value.ToLocal().Date);
            if (IsImportPurchase) ExpressionObject = ExpressionObject.And(x => x.IsImportPurchase == IsImportPurchase);
            if (GRNStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == GRNStatus);
            if (GRNStatuses.Any()) ExpressionObject = ExpressionObject.And(x => GRNStatuses.Contains(x.Status));
            return ExpressionObject;
        }
        public override IQueryable<GoodsReceiveNote> IncludeParents(IQueryable<GoodsReceiveNote> queryable)
        {
            return queryable.Include(x => x.Supplier).Include(x => x.Store)
                .Include(x => x.GoodsReceiveNoteDetails)
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
