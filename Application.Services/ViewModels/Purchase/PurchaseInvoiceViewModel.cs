using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Purchase
{
    public class PurchaseInvoiceViewModel
    {
        public Guid Id { get; set; }
        public string? PurchaseInvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string? Grnno { get; set; }
        public string? Ponumber { get; set; }
        public Guid StoreId { get; set; }
        public Guid? CurrencyId { get; set; }
        public Guid SupplierId { get; set; }
        public string? SupplierInvoiceNo { get; set; }
        public DateTime? SupplierInvoiceDate { get; set; }
        public int PaymentTermInDays { get; set; }
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public decimal WeightVariance { get; set; }
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
        public decimal PaidAmount { get; set; }
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
        public bool IsLcAdjusted { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? CheckedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public Guid FinancialYearId { get; set; }

        public virtual StoreViewModel? Store { get; set; }
        public virtual SupplierViewModel? Supplier { get; set; }
        public virtual ICollection<PurchaseInvoiceDetailViewModel>? PurchaseInvoiceDetails { get; set; }
    }
}
