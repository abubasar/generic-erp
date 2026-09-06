using Application.Core.Common;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Production
{
    public class BillOfMaterialRequestModel : BaseRequestModel<BillOfMaterial>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? FinishedProductId { get; set; }
        public string? BomNo { get; set; }
        public string? FormulationNo { get; set; }
        public int? BOMStatus { get; set; }
        public override Expression<Func<BillOfMaterial, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = x =>
                    x.BomNo.ToLower().Contains(Keyword) || x.FormulationNo.ToLower().Contains(Keyword);
            }
            if (FinishedProductId.HasValue) ExpressionObject = ExpressionObject.And(x => x.FinishedProductId == FinishedProductId);
            if (!string.IsNullOrWhiteSpace(FormulationNo)) ExpressionObject = ExpressionObject.And(x => x.FormulationNo == FormulationNo);
            if (!string.IsNullOrWhiteSpace(BomNo)) ExpressionObject = ExpressionObject.And(x => x.BomNo == BomNo);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.CreatedOn.Date >= FromDate.Value.Date && x.CreatedOn.Date <= ToDate.Value.AddDays(1).Date);
            if (BOMStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == BOMStatus);
            return ExpressionObject;
        }
        public override IQueryable<BillOfMaterial> IncludeParents(IQueryable<BillOfMaterial> queryable)
        {
            return queryable.Include(x => x.FinishedProduct).ThenInclude(x => x.PackSize).Include(x => x.BillOfMaterialDetails.OrderBy(x => x.RawMaterial.Name))
                .ThenInclude(x => x.RawMaterial).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
