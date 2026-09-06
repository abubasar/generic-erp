using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Accounts
{
    public class FundTransferRequestModel : BaseRequestModel<FundTransfer>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? TransferFromAccountId { get; set; }
        public Guid? TransferToAccountId { get; set; }
        public int? FundTransferStatus { get; set; }
        public override Expression<Func<FundTransfer, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x =>
                    x.FundTransferNo.ToLower().Contains(Keyword));
            }
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.FundTransferDate.Date >= FromDate.Value.ToLocal().Date && x.FundTransferDate.Date <= ToDate.Value.ToLocal().Date);
            if (FundTransferStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == FundTransferStatus);
            if (CostCenterId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CostCenterId == CostCenterId.Value);
            if (TransferFromAccountId.HasValue) ExpressionObject = ExpressionObject.And(x => x.TransferFromAccountId == TransferFromAccountId.Value);
            if (TransferToAccountId.HasValue) ExpressionObject = ExpressionObject.And(x => x.TransferToAccountId == TransferToAccountId.Value);
            return ExpressionObject;
        }
        public override IQueryable<FundTransfer> IncludeParents(IQueryable<FundTransfer> queryable)
        {
            return queryable.Include(x => x.CostCenter).Include(x => x.FundTransferTransactionType).Include(x => x.TransferFromAccount).Include(x => x.TransferToAccount);
        }
    }
}
