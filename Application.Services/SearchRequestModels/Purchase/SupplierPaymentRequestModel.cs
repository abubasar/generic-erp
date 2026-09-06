using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Application.Services.SearchRequestModels.Purchase
{
    public class SupplierPaymentRequestModel : BaseRequestModel<SupplierPayment>
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? TransactionNumber { get; set; }
        public Guid? PaymentModeId { get; set; }
        public Guid? SupplierId { get; set; }
        public Guid? CostCenterId { get; set; }
        public int? SupplierPaymentStatus { get; set; }
        public List<int> SupplierPaymentStatuses { get; set; } = new();
        public int? SupplierPaymentType { get; set; }
        public override Expression<Func<SupplierPayment, bool>> GetExpression()
        {
            if (!string.IsNullOrWhiteSpace(Keyword)) ExpressionObject = x => x.Code.Contains(Keyword) || x.Ponumber.ToLower().Contains(Keyword);
            if (!string.IsNullOrWhiteSpace(TransactionNumber)) ExpressionObject = x => x.TransactionNumber.Contains(TransactionNumber);
            if (SupplierId.HasValue) ExpressionObject = ExpressionObject.And(x => x.SupplierId == SupplierId);
            if (CostCenterId.HasValue) ExpressionObject = ExpressionObject.And(x => x.CostCenterId == CostCenterId);
            if (PaymentModeId.HasValue) ExpressionObject = ExpressionObject.And(x => x.PaymentModeId == PaymentModeId);
            if (FromDate.HasValue && ToDate.HasValue)
                ExpressionObject = ExpressionObject.And(x => x.PaymentDate.Date >= FromDate.Value.ToLocal().Date && x.PaymentDate.Date <= ToDate.Value.ToLocal().Date);
            if (SupplierPaymentStatuses.Any()) ExpressionObject = ExpressionObject.And(x => SupplierPaymentStatuses.Contains(x.Status));
            if (SupplierPaymentStatus.HasValue) ExpressionObject = ExpressionObject.And(x => x.Status == SupplierPaymentStatus);
            if (SupplierPaymentType.HasValue) ExpressionObject = ExpressionObject.And(x => x.SupplierPaymentType == SupplierPaymentType);
            return ExpressionObject;
        }
        public override IQueryable<SupplierPayment> IncludeParents(IQueryable<SupplierPayment> queryable)
        {
            return queryable.Include(x => x.CostCenter).Include(x => x.FundTransferTransactionType).Include(x => x.Supplier).Include(x => x.PaymentMode).Include(x => x.SupplierPaymentDetails).ThenInclude(x => x.Account);
        }
    }
}
