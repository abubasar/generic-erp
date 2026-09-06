using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Accounts
{
    public class JournalEntryRequestModel : BaseRequestModel<JournalEntry>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? CostCenterId { get; set; }
        public string? VoucherNo { get; set; }
        public int? JournalEntryStatus { get; set; }
        public override Expression<Func<JournalEntry, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(VoucherNo))
                ExpressionObject = x => x.VoucherNo == VoucherNo;
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                ExpressionObject = ExpressionObject.And(x =>
                    x.VoucherNo.ToLower().Contains(Keyword) || x.Remark.ToLower().Contains(Keyword)
                    || x.JournalEntryDetails.Any(x => x.Account.Name.Contains(Keyword)));
            }
            if (CostCenterId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CostCenterId == CostCenterId.Value);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.VoucherDate.Date >= FromDate.Value.ToLocal().Date && x.VoucherDate.Date <= ToDate.Value.ToLocal().Date);
            if (JournalEntryStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == JournalEntryStatus);
            return ExpressionObject;
        }
        public override IQueryable<JournalEntry> IncludeParents(IQueryable<JournalEntry> queryable)
        {
            return queryable.Include(x => x.CostCenter).Include(x => x.JournalEntryDetails).ThenInclude(x => x.Account);
        }
    }
}
