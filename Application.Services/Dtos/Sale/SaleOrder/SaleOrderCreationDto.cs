namespace Application.Services.Dtos.Sale.SaleOrder
{
    public class SaleOrderCreationDto
    {
        public string? QuotationNo { get; set; }
        public int Transport { get; set; }
        public DateTime OrderDate { get; set; }
        public Guid CustomerId { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal LimitAvailed { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string? ReferenceNo { get; set; }
        public Guid StoreId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPercentageDiscountAmount { get; set; }
        public decimal OfferDiscount { get; set; }
        public decimal OtherDiscount { get; set; }
        public decimal Total { get; set; }
        public decimal TransportationCost { get; set; }
        public decimal DepoCharge { get; set; }
        public decimal NetTotal { get; set; }
        public bool IsMailSent { get; set; }
        public string? MoneyReceiptNo { get; set; }
        public string? Remark { get; set; }
        public int PaymentTerm { get; set; }
        public Guid? CustomerTerritoryId { get; set; }
        public Guid? CustomerWiseProductDiscountId { get; set; }
        public Guid? DiscountProductWiseId { get; set; }

        public virtual ICollection<SaleOrderDetailCreationDto>? SaleOrderDetails { get; set; }
    }
}
