using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Accounts.AccountsReceivable
{
    public class SaleInvoiceRequestModel : BaseRequestModel<SaleInvoice>
    {
        /* ReportType is being used to indentify Pdf or excel in report design*/
        public int? ReportType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SaleOrderNo { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? CustomerZoneId { get; set; }
        public Guid? CustomerAreaId { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public Guid? CustomerTerritoryId { get; set; }
        public int? SaleInvoiceStatus { get; set; }
        public List<int> SaleInvoiceStatuses { get; set; } = new();
        public override Expression<Func<SaleInvoice, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = x =>
                   x.InvoiceNo.ToLower().Contains(Keyword) || x.InvoiceNo.ToLower().Contains(Keyword) || x.DeliveryNoteNo.ToLower().Contains(Keyword)
                   || x.SaleOrderNo.ToLower().Contains(Keyword)
                   || x.ReferenceNo.ToLower().Contains(Keyword);
            }
            if (StoreId.HasValue) ExpressionObject = ExpressionObject.And(x => x.StoreId == StoreId.Value);
            if (!string.IsNullOrWhiteSpace(SaleOrderNo))
                ExpressionObject = ExpressionObject.And(x => x.SaleOrderNo.ToLower() == SaleOrderNo.ToLower());
            if (CustomerZoneId.HasValue) ExpressionObject = ExpressionObject.And(x => x.Customer.CustomerZoneId == CustomerZoneId.Value);
            if (CustomerAreaId.HasValue) ExpressionObject = ExpressionObject.And(x => x.Customer.CustomerAreaId == CustomerAreaId.Value);
            if (CustomerMarketingOfficerId.HasValue) ExpressionObject = ExpressionObject.And(x => x.Customer.CustomerMarketingOfficerId == CustomerMarketingOfficerId.Value);
            if (CustomerTerritoryId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CustomerTerritoryId == CustomerTerritoryId.Value);
            if (CustomerId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CustomerId == CustomerId.Value);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.InvoiceDate.Date >= FromDate.Value.ToLocal().Date && x.InvoiceDate.Date <= ToDate.Value.ToLocal().Date);
            if (SaleInvoiceStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == SaleInvoiceStatus);
            if (SaleInvoiceStatuses.Any()) ExpressionObject = ExpressionObject.And(x => SaleInvoiceStatuses.Contains(x.Status));
            return ExpressionObject;
        }
        public override IQueryable<SaleInvoice> IncludeParents(IQueryable<SaleInvoice> queryable)
        {
            return queryable.Include(x => x.Customer).Include(x => x.Store).Include(x => x.CustomerTerritory)
                .Include(x => x.SaleInvoiceDetails)
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
