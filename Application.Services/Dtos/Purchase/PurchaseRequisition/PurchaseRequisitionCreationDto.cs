namespace Application.Services.Dtos.Purchase.PurchaseRequisition
{
    public class PurchaseRequisitionCreationDto
    {
        public DateTime RequisitionDate { get; set; }
        public Guid StoreId { get; set; }
        public Guid DepartmentId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public int Priority { get; set; }
        public int PaymentTermInDays { get; set; }
        public int PaymentMode { get; set; }
        public int Transport { get; set; }
        public string? TermAndCondition { get; set; }
        public Guid? CurrencyId { get; set; }
        public int ImportPurchaseIncoTerm { get; set; }
        public int ImportPurchasePaymentTerm { get; set; }
        public string? Remark { get; set; }
        public virtual ICollection<PurchaseRequisitionDetailCreationDto>? PurchaseRequisitionDetails { get; set; }
    }
}
