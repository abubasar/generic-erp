namespace Application.Services.Dtos.Purchase.PurchaseInvoice
{
    public class PurchaseInvoiceCreationDto
    {
        public DateTime InvoiceDate { get; set; }
        public string? Grnno { get; set; }
        public string? Ponumber { get; set; }
        public Guid StoreId { get; set; }
        public Guid? CurrencyId { get; set; }
        public Guid SupplierId { get; set; }
        public string? SupplierInvoiceNo { get; set; }
        public DateTime? SupplierInvoiceDate { get; set; }
        public int PaymentTermInDays { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TransportationCost { get; set; }
        public decimal TotalVat { get; set; }
        public decimal Total { get; set; }
        public string? SupplierPaymentCode { get; set; }
        public decimal AdvancePaymentAmount { get; set; }
        public decimal TotalGrnAdjustmentAmount { get; set; }
        public decimal PurchaseOrderTotal { get; set; }
        public decimal NetPayable { get; set; }
        public bool IsImportPurchase { get; set; }
        public string? ProformaInvoiceNo { get; set; }
        public string? LcNumber { get; set; }
        public decimal ExchangeRate { get; set; }
        public int ImportPurchaseIncoTerm { get; set; }
        public int ImportPurchasePaymentTerm { get; set; }
        public string? BillOfEntryNo { get; set; }
        public DateTime? BillOfEntryDate { get; set; }
        public string? PortOfLoading { get; set; }
        public string? PortOfDestination { get; set; }
        public decimal AdditionalLandedCost { get; set; }
        public decimal AdjustmentValue { get; set; }
        public string? Remark { get; set; }

        public virtual ICollection<PurchaseInvoiceDetailCreationDto>? PurchaseInvoiceDetails { get; set; }
    }
}
