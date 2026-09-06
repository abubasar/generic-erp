using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Purchase
{
    public class GoodsReceiveNoteDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid GoodsReceiveNoteId { get; set; }
        public Guid? PurchaseOrderDetailId { get; set; }
        public Guid ProductId { get; set; }
        public int Poquantity { get; set; }
        public int Grnquantity { get; set; }
        public int BagWeightDeductionQuantity { get; set; }
        public int NumberOfBagQuantity { get; set; }
        public int NetQuantity { get; set; }
        public string? BatchNo { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int RejectedQuantity { get; set; }
        public string? RejectionReason { get; set; }
        public decimal CurrencyRate { get; set; }
        public decimal Rate { get; set; }
        public decimal RateAfterBagWeightDeduction { get; set; }
        public decimal CurrencyAmount { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual ProductViewModel? Product { get; set; }
    }
}
