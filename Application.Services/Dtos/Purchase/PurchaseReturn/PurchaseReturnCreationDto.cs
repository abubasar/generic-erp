namespace Application.Services.Dtos.Purchase.PurchaseReturn
{
    public class PurchaseReturnCreationDto
    {
        public DateTime PurchaseReturnDate { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Grnno { get; set; }
        public Guid SupplierId { get; set; }
        public Guid StoreId { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Remark { get; set; }
        public virtual ICollection<PurchaseReturnDetailCreationDto>? PurchaseReturnDetails { get; set; }
    }
}
