using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Accounts
{
    public class ReceiveVoucherRequestModel : BaseRequestModel<ReceiveVoucher>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? TransactionNumber { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? PaymentModeId { get; set; }
        public Guid? AccountId { get; set; }
        public string? VoucherNo { get; set; }
        public int? ReceiveVoucherStatus { get; set; }
        public int? AccountTransactionType { get; set; }
        public override Expression<Func<ReceiveVoucher, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(VoucherNo)) ExpressionObject = x => x.VoucherNo == VoucherNo;
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = ExpressionObject.And(x => x.VoucherNo.ToLower().Contains(Keyword));
            if (!string.IsNullOrWhiteSpace(TransactionNumber)) ExpressionObject = x => x.TransactionNumber.Contains(TransactionNumber);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.VoucherDate.Date >= FromDate.Value.ToLocal().Date && x.VoucherDate.Date <= ToDate.Value.ToLocal().Date);
            if (ReceiveVoucherStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == ReceiveVoucherStatus);
            if (CostCenterId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CostCenterId == CostCenterId.Value);
            if (PaymentModeId.HasValue) ExpressionObject = ExpressionObject.And(x => x.PaymentModeId == PaymentModeId.Value);
            if (AccountId.HasValue) ExpressionObject = ExpressionObject.And(x => x.ReceiveVoucherDetails.Any(x => x.AccountId == AccountId.Value));
            return ExpressionObject;
        }
        public override IQueryable<ReceiveVoucher> IncludeParents(IQueryable<ReceiveVoucher> queryable)
        {
            return queryable.Include(x => x.CostCenter).Include(x => x.FundTransferTransactionType).Include(x => x.CashBankAccount).Include(x => x.PaymentMode).Include(blog => blog.ReceiveVoucherDetails).ThenInclude(x => x.Account);
        }
    }
}
