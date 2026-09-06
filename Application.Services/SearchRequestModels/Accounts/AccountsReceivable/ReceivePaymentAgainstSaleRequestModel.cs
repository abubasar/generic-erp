using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Accounts.AccountsReceivable
{
    public class ReceivePaymentAgainstSaleRequestModel : BaseRequestModel<ReceivePaymentAgainstSale>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public Guid? CostCenterId { get; set; }
        public int? ReceivePaymentStatus { get; set; }
        public override Expression<Func<ReceivePaymentAgainstSale, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Code.Contains(Keyword);
            if (CustomerId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CustomerId == CustomerId);
            if (CustomerMarketingOfficerId.HasValue) ExpressionObject = ExpressionObject.And(x => x.Customer.CustomerMarketingOfficerId == CustomerMarketingOfficerId.Value);
            if (CostCenterId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CostCenterId == CostCenterId);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.PaymentDate.Date >= FromDate.Value.ToLocal().Date && x.PaymentDate.Date <= ToDate.Value.ToLocal().Date);
            if (ReceivePaymentStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == ReceivePaymentStatus);
            return ExpressionObject;
        }
        public override IQueryable<ReceivePaymentAgainstSale> IncludeParents(IQueryable<ReceivePaymentAgainstSale> queryable)
        {
            return queryable.Include(x => x.CostCenter).Include(x => x.ToAccount).Include(x => x.Customer);
        }
    }
}
