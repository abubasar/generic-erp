using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Accounts
{
    public class VoucherEntryRequestModel : BaseRequestModel<VoucherEntry>
    {
        public int ReportType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? PaymentModeId { get; set; }
        public Guid? AccountId { get; set; }
        public string? VoucherNo { get; set; }
        public int? VoucherType { get; set; }
        public int? VoucherEntryStatus { get; set; }
        public int? AccountTransactionType { get; set; }
        public override Expression<Func<VoucherEntry, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(VoucherNo))
                ExpressionObject = x => x.VoucherNo == VoucherNo;
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x =>
                    x.VoucherNo.ToLower().Contains(Keyword));
            }
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.VoucherDate.Date >= FromDate.Value.ToLocal().Date && x.VoucherDate.Date <= ToDate.Value.ToLocal().Date);
            if (VoucherEntryStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == VoucherEntryStatus);
            if (CostCenterId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CostCenterId == CostCenterId.Value);
            if (PaymentModeId.HasValue) ExpressionObject = ExpressionObject.And(x => x.PaymentModeId == PaymentModeId.Value);
            if (AccountId.HasValue) ExpressionObject = ExpressionObject.And(x => x.VoucherEntryDetails.Any(x => x.AccountId == AccountId.Value));
            if (VoucherType.HasValue) ExpressionObject = ExpressionObject.And(x => x.VoucherType == VoucherType.Value);
            return ExpressionObject;
        }
        public override IQueryable<VoucherEntry> IncludeParents(IQueryable<VoucherEntry> queryable)
        {
            return queryable.Include(x => x.CostCenter).Include(x => x.CashBankAccount).Include(x => x.PaymentMode).Include(blog => blog.VoucherEntryDetails).ThenInclude(x => x.Account);
        }
    }
}
