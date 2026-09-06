namespace Application.Services.Dtos.Sale.DeliveryNote
{
    public class DeliveryNoteCreationDto
    {
        public string? SaleOrderNo { get; set; }
        public DateTime? OrderDate { get; set; }
        public Guid CustomerId { get; set; }
        public Guid StoreId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string? DeliveryPlace { get; set; }
        public int Transport { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal LimitAvailed { get; set; }
        public string? ReferenceNo { get; set; }
        public string? TruckNo { get; set; }
        public string? DriverName { get; set; }
        public string? DriverContactNo { get; set; }
        public string? MoneyReceiptNo { get; set; }
        public string? Remark { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal OfferDiscount { get; set; }
        public decimal OtherDiscount { get; set; }
        public decimal Total { get; set; }
        public decimal TransportationCost { get; set; }
        public decimal DepoCharge { get; set; }
        public decimal NetTotal { get; set; }

        public virtual ICollection<DeliveryNoteDetailCreationDto>? DeliveryNoteDetails { get; set; }
    }
}
