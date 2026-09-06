using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Purchase
{
    public class PurchaseInvoiceRequestModel : BaseRequestModel<PurchaseInvoice>
    {
        /* ReportType is being used to indentify Pdf or excel in report design*/
        public int ReportType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Grnno { get; set; }
        public Guid? SupplierId { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? StoreId { get; set; }
        public bool IsImportPurchase { get; set; }
        public string? LcNumber { get; set; }
        public int? PurchaseInvoiceStatus { get; set; }
        public List<int> PurchaseInvoiceStatuses { get; set; } = new();
        public override Expression<Func<PurchaseInvoice, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = x =>
                   x.PurchaseInvoiceNo.ToLower().Contains(Keyword) || x.Grnno.ToLower().Contains(Keyword)
                   || x.Ponumber.ToLower().Contains(Keyword) || x.SupplierInvoiceNo.ToLower().Contains(Keyword);
            }
            if (!string.IsNullOrWhiteSpace(Grnno)) ExpressionObject = ExpressionObject.And(x => x.Grnno.Contains(Grnno));
            if (!string.IsNullOrWhiteSpace(LcNumber)) ExpressionObject = ExpressionObject.And(x => x.LcNumber == LcNumber);
            if (SupplierId.HasValue) ExpressionObject = ExpressionObject.And(x => x.SupplierId == SupplierId);
            if (StoreId.HasValue) ExpressionObject = ExpressionObject.And(x => x.StoreId == StoreId);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.InvoiceDate.Date >= FromDate.Value.ToLocal().Date && x.InvoiceDate.Date <= ToDate.Value.ToLocal().Date);
            if (IsImportPurchase) ExpressionObject = ExpressionObject.And(x => x.IsImportPurchase == IsImportPurchase);
            if (PurchaseInvoiceStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == PurchaseInvoiceStatus);
            if (PurchaseInvoiceStatuses.Any()) ExpressionObject = ExpressionObject.And(x => PurchaseInvoiceStatuses.Contains(x.Status));
            return ExpressionObject;
        }
        public override IQueryable<PurchaseInvoice> IncludeParents(IQueryable<PurchaseInvoice> queryable)
        {
            return queryable.Include(x => x.Supplier).Include(x => x.Store)
                .Include(x => x.PurchaseInvoiceDetails)
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
