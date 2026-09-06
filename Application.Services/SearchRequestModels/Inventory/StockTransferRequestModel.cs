using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Inventory
{
    public class StockTransferRequestModel : BaseRequestModel<StockTransfer>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? StockTransferStatus { get; set; }
        public override Expression<Func<StockTransfer, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = x =>
                    x.TransferNo.ToLower().Contains(Keyword) || x.TransferNo.ToLower().Contains(Keyword);
            }
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.TransferDate.Date >= FromDate.Value.ToLocal().Date && x.TransferDate.Date <= ToDate.Value.ToLocal().Date);
            if (StockTransferStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == StockTransferStatus);
            return ExpressionObject;
        }
        public override IQueryable<StockTransfer> IncludeParents(IQueryable<StockTransfer> queryable)
        {
            return queryable.Include(x => x.Source).Include(x => x.Destination)
                .Include(x => x.StockTransferDetails).ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit);
        }
    }
}
