using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Production
{
    public class ManufacturingOrderRequestModel : BaseRequestModel<ManufacturingOrder>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? FinishedProductId { get; set; }
        public string? ManufacturingOrderNo { get; set; }
        public string? BomNo { get; set; }
        public string? FormulationNo { get; set; }
        public int? ManufacturingOrderStatus { get; set; }
        public override Expression<Func<ManufacturingOrder, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = x =>
                    x.BomNo.ToLower().Contains(Keyword) || x.ManufacturingOrderNo.ToLower().Contains(Keyword) || x.FormulationNo.ToLower().Contains(Keyword);
            }
            if (FinishedProductId.HasValue) ExpressionObject = ExpressionObject.And(x => x.FinishedProductId == FinishedProductId);
            if (!string.IsNullOrWhiteSpace(ManufacturingOrderNo)) ExpressionObject = ExpressionObject.And(x => x.ManufacturingOrderNo == ManufacturingOrderNo);
            if (!string.IsNullOrWhiteSpace(FormulationNo)) ExpressionObject = ExpressionObject.And(x => x.FormulationNo == FormulationNo);
            if (!string.IsNullOrWhiteSpace(BomNo)) ExpressionObject = ExpressionObject.And(x => x.BomNo == BomNo);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.ScheduledDate.Date >= FromDate.Value.ToLocal().Date && x.ScheduledDate.Date <= ToDate.Value.ToLocal().Date);
            if (ManufacturingOrderStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == ManufacturingOrderStatus);
            return ExpressionObject;
        }
        public override IQueryable<ManufacturingOrder> IncludeParents(IQueryable<ManufacturingOrder> queryable)
        {
            return queryable.Include(x => x.FinishedProduct).ThenInclude(x => x.PackSize).Include(x => x.RawMaterialStore)
                .Include(x => x.ManufacturingOrderDetails.OrderBy(x => x.RawMaterial.Name))
                .ThenInclude(x => x.RawMaterial).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
