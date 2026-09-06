namespace Application.Services.Dtos.Purchase.SupplierPayment
{
    public class SupplierPaymentCreationDto
    {
        public int SupplierPaymentType { get; set; }
        public DateTime PaymentDate { get; set; }
        public Guid? FundTransferTransactionTypeId { get; set; }
        public string? TransactionNumber { get; set; }
        public string? Ponumber { get; set; }
        public Guid SupplierId { get; set; }
        public Guid PaymentModeId { get; set; }
        public decimal TotalAmount { get; set; }
        public Guid CostCenterId { get; set; }
        public string? Remark { get; set; }
        public virtual ICollection<SupplierPaymentDetailCreationDto>? SupplierPaymentDetails { get; set; }
    }
}
