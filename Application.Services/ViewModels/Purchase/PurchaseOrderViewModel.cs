using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Purchase
{
    public class PurchaseOrderViewModel
    {
        public Guid Id { get; set; }
        public string? RequisitionNo { get; set; }
        public string? QuotationNo { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Ponumber { get; set; }
        public DateTime Podate { get; set; }
        public Guid? DeliveryPlaceId { get; set; }
        public Guid? PaymentMethodId { get; set; }
        public Guid StoreId { get; set; }
        public Guid SupplierId { get; set; }
        public string? ProductOrigin { get; set; }
        public string? PackagingType { get; set; }
        public string? ExpiryTime { get; set; }
        public int Transport { get; set; }
        public string? TransportName { get; set; }
        public int PaymentTermInDays { get; set; }
        public int DeliveryTermInDays { get; set; }
        public DateTime DeliveryDate { get; set; }
        public int PaymentMode { get; set; }
        public decimal WeightVariance { get; set; }
        public string? PaymentModeName { get; set; }
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string? TermAndCondition { get; set; }
        public bool IsImportPurchase { get; set; }
        public string? ProformaInvoiceNo { get; set; }
        public string? LcNumber { get; set; }
        public decimal ExchangeRate { get; set; }
        public Guid? CurrencyId { get; set; }
        public int ImportPurchaseIncoTerm { get; set; }
        public int ImportPurchasePaymentTerm { get; set; }
        public string? PortOfLoading { get; set; }
        public string? PortOfDestination { get; set; }
        public decimal AdditionalLandedCost { get; set; }
        public string? Remark { get; set; }
        public bool IsPartialDelivery { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? CheckedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public Guid FinancialYearId { get; set; }
        public virtual DeliveryPlaceViewModel? DeliveryPlace { get; set; }
        public virtual StoreViewModel? Store { get; set; }
        public virtual SupplierViewModel? Supplier { get; set; }
        public virtual ICollection<PurchaseOrderDetailViewModel>? PurchaseOrderDetails { get; set; }
    }
}
