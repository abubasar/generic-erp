using Application.Core.Common;
using Application.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Accounts.AccountsPayable
{
    public class SupplierPaymentAgainstPurchaseRequestModel : BaseRequestModel<SupplierPaymentAgainstPurchase>
    {
        public Guid? SupplierId { get; set; }
        public Guid? CostCenterId { get; set; }
        public int? SupplierPaymentStatus { get; set; }
        public override Expression<Func<SupplierPaymentAgainstPurchase, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Code.Contains(Keyword);
            if (SupplierId.HasValue) ExpressionObject = ExpressionObject.And(x => x.SupplierId == SupplierId);
            if (CostCenterId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CostCenterId == CostCenterId);
            if (SupplierPaymentStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == SupplierPaymentStatus);
            return ExpressionObject;
        }
        public override IQueryable<SupplierPaymentAgainstPurchase> IncludeParents(IQueryable<SupplierPaymentAgainstPurchase> queryable)
        {
            return queryable.Include(x => x.CostCenter).Include(x => x.FromAccount).Include(x => x.PurchaseInvoice).ThenInclude(x => x.Supplier);
        }
    }
}
