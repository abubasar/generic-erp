using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Purchase
{
    public class PurchaseRequisitionViewModel
    {
        public Guid Id { get; set; }
        public string? RequisitionNo { get; set; }
        public DateTime RequisitionDate { get; set; }
        public Guid StoreId { get; set; }
        public Guid DepartmentId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? RequestBy { get; set; }
        public string? RequestByName { get; set; }
        public int Priority { get; set; }
        public string? PriorityName { get; set; }
        public int PaymentTermInDays { get; set; }
        public int Transport { get; set; }
        public int PaymentMode { get; set; }
        public string? TermAndCondition { get; set; }
        public Guid? CurrencyId { get; set; }
        public int ImportPurchaseIncoTerm { get; set; }
        public int ImportPurchasePaymentTerm { get; set; }
        public string? Remark { get; set; }
        public int RequisitionStatus { get; set; }
        public string? RequisitionStatusName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? CheckedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public Guid FinancialYearId { get; set; }

        public virtual DepartmentViewModel? Department { get; set; }
        public virtual StoreViewModel? Store { get; set; }
        public virtual ICollection<PurchaseRequisitionDetailViewModel>? PurchaseRequisitionDetails { get; set; }
    }
}
