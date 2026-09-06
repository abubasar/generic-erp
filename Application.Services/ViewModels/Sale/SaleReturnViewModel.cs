using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Sale
{
    public class SaleReturnViewModel
    {
        public Guid Id { get; set; }
        public string? DeliveryNoteNo { get; set; }
        public string? InvoiceNo { get; set; }
        public string? SaleReturnNo { get; set; }
        public DateTime SaleReturnDate { get; set; }
        public string? ReferenceNo { get; set; }
        public Guid StoreId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public Guid? CustomerTerritoryId { get; set; }
        public decimal TotalVat { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalPercentageDiscountAmount { get; set; }
        public decimal OtherDiscount { get; set; }
        public decimal Total { get; set; }
        public string? Remark { get; set; }
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
        public virtual ICollection<SaleReturnDetailViewModel>? SaleReturnDetails { get; set; }
    }
}
