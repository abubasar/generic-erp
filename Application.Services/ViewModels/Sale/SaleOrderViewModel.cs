using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Sale
{
    public class SaleOrderViewModel
    {
        public Guid Id { get; set; }
        public string? SaleOrderNo { get; set; }
        public string? QuotationNo { get; set; }
        public int Transport { get; set; }
        public string? TransportName { get; set; }
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
        public string? PaymentTermName { get; set; }
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public string? CheckedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public Guid FinancialYearId { get; set; }

        public virtual CustomerViewModel? Customer { get; set; }
        public virtual TerritoryViewModel? CustomerTerritory { get; set; }
        public virtual StoreViewModel? Store { get; set; }
        public virtual ICollection<SaleOrderDetailViewModel>? SaleOrderDetails { get; set; }
    }
}
