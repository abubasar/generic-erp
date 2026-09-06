using Application.Core.Common;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Production
{
    public class ProductionRequestModel : BaseRequestModel<Application.Core.Entities.Production>
    {
        /* ReportType is being used to indentify Pdf or excel in report design*/
        public int? ReportType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? FinishedProductId { get; set; }
        public Guid? FgstoreId { get; set; }
        public int? ProductionStatus { get; set; }
        public override Expression<Func<Core.Entities.Production, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = x =>
                    x.ProductionNo.ToLower().Contains(Keyword) || x.BomNo.ToLower().Contains(Keyword) || x.ManufacturingOrderNo.ToLower().Contains(Keyword) || x.FormulationNo.ToLower().Contains(Keyword) || x.BatchNo.ToLower().Contains(Keyword);
            }
            if (FgstoreId.HasValue) ExpressionObject = ExpressionObject.And(x => x.FgstoreId == FgstoreId);
            if (FinishedProductId.HasValue) ExpressionObject = ExpressionObject.And(x => x.FinishedProductId == FinishedProductId);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.ProductionDate.Date >= FromDate.Value.ToLocal().Date && x.ProductionDate.Date <= ToDate.Value.ToLocal().Date);
            if (ProductionStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == ProductionStatus);
            return ExpressionObject;
        }
        public override IQueryable<Core.Entities.Production> IncludeParents(IQueryable<Core.Entities.Production> queryable)
        {
            return queryable.Include(x => x.FinishedProduct).ThenInclude(x => x.PackSize).Include(x => x.Fgstore).Include(x => x.Shift).Include(x => x.Machine)
                .Include(x => x.ProductionDetails.OrderBy(x => x.RawMaterial.Name))
                .ThenInclude(x => x.RawMaterial).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
