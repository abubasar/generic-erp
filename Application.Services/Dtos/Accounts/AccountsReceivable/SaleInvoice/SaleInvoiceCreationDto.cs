namespace Application.Services.Dtos.Accounts.AccountsReceivable.SaleInvoice
{
    public class SaleInvoiceCreationDto
    {
        public DateTime InvoiceDate { get; set; }
        public string? DeliveryNoteNo { get; set; }
        public string? SaleOrderNo { get; set; }
        public DateTime? OrderDate { get; set; }
        public Guid CustomerId { get; set; }
        public Guid StoreId { get; set; }
        public int Transport { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalPercentageDiscountAmount { get; set; }
        public decimal OfferDiscount { get; set; }
        public decimal OtherDiscount { get; set; }
        public decimal Total { get; set; }
        public decimal TotalVat { get; set; }
        public decimal TransportationCost { get; set; }
        public decimal DepoCharge { get; set; }
        public decimal NetTotal { get; set; }
        public string? ReferenceNo { get; set; }
        public string? BankDetails { get; set; }
        public string? TermAndCondition { get; set; }
        public string? MoneyReceiptNo { get; set; }
        public string? Remark { get; set; }
        public int PaymentTerm { get; set; }
        public Guid? CustomerTerritoryId { get; set; }

        public virtual ICollection<SaleInvoiceDetailCreationDto>? SaleInvoiceDetails { get; set; }
    }
}
