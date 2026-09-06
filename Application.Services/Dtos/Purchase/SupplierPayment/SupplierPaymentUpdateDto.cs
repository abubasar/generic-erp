namespace Application.Services.Dtos.Purchase.SupplierPayment
{
    public class SupplierPaymentUpdateDto : SupplierPaymentCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedSupplierPaymentDetailIds { get; set; }
        public new ICollection<SupplierPaymentDetailUpdateDto>? SupplierPaymentDetails { get; set; }
    }
}
