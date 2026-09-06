using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Purchase
{
    public class GoodsReceiveNoteViewModel
    {
        public Guid Id { get; set; }
        public string? Grnno { get; set; }
        public DateTime Grndate { get; set; }
        public string? Ponumber { get; set; }
        public Guid StoreId { get; set; }
        public Guid? CurrencyId { get; set; }
        public Guid SupplierId { get; set; }
        public string? ChallanNo { get; set; }
        public DateTime? ChallanDate { get; set; }
        public string? TruckNo { get; set; }
        public string? DriverName { get; set; }
        public string? DriverContactNo { get; set; }
        public int? Transport { get; set; }
        public int PaymentTermInDays { get; set; }
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TransportationCost { get; set; }
        public decimal Total { get; set; }
        public decimal TotalGrnAdjustmentAmount { get; set; }
        public decimal PurchaseOrderTotal { get; set; }
        public bool IsImportPurchase { get; set; }
        public string? ProformaInvoiceNo { get; set; }
        public string? LcNumber { get; set; }
        public decimal ExchangeRate { get; set; }
        public int ImportPurchaseIncoTerm { get; set; }
        public int ImportPurchasePaymentTerm { get; set; }
        public string? PortOfLoading { get; set; }
        public string? PortOfDestination { get; set; }
        public decimal AdditionalLandedCost { get; set; }
        public string? Remark { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? CheckedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public Guid FinancialYearId { get; set; }

        public virtual StoreViewModel? Store { get; set; }
        public virtual SupplierViewModel? Supplier { get; set; }
        public virtual ICollection<GoodsReceiveNoteDetailViewModel>? GoodsReceiveNoteDetails { get; set; }
    }
}
