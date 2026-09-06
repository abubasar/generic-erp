using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Accounts.AccountsReceivable
{
    public class ReceivePaymentRequestModel : BaseRequestModel<ReceivePayment>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? TransactionNumber { get; set; }
        public Guid? PaymentModeId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public Guid? CostCenterId { get; set; }
        public int? ReceivePaymentStatus { get; set; }
        public List<int> ReceivePaymentStatuses { get; set; } = new();
        public override Expression<Func<ReceivePayment, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Code.Contains(Keyword);
            if (!string.IsNullOrWhiteSpace(TransactionNumber)) ExpressionObject = x => x.TransactionNumber.Contains(TransactionNumber);
            if (CustomerId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CustomerId == CustomerId);
            if (CustomerMarketingOfficerId.HasValue) ExpressionObject = ExpressionObject.And(x => x.Customer.CustomerMarketingOfficerId == CustomerMarketingOfficerId.Value);
            if (CostCenterId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CostCenterId == CostCenterId);
            if (PaymentModeId.HasValue) ExpressionObject = ExpressionObject.And(x => x.PaymentModeId == PaymentModeId);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.PaymentDate.Date >= FromDate.Value.ToLocal().Date && x.PaymentDate.Date <= ToDate.Value.ToLocal().Date);
            if (ReceivePaymentStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == ReceivePaymentStatus);
            if (ReceivePaymentStatuses.Any()) ExpressionObject = ExpressionObject.And(x => ReceivePaymentStatuses.Contains(x.Status));
            return ExpressionObject;
        }
        public override IQueryable<ReceivePayment> IncludeParents(IQueryable<ReceivePayment> queryable)
        {
            return queryable.Include(x => x.CostCenter).Include(x => x.Customer).Include(x => x.FundTransferTransactionType).Include(x => x.PaymentMode).Include(x => x.ReceivePaymentDetails).ThenInclude(x => x.Account);
        }
    }
}
