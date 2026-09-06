namespace Application.Services.Dtos.Sale.DeliveryNote
{
    public class DeliveryNoteDetailCreationDto
    {
        public Guid? SaleOrderDetailId { get; set; }
        public Guid ProductId { get; set; }
        public int BagWeight { get; set; }
        public decimal Rate { get; set; }
        public decimal NetRate { get; set; }
        public decimal DiscountPerUnit { get; set; }
        public decimal OfferDiscountPerUnit { get; set; }
        public decimal InvoiceDiscountPerUnit { get; set; }
        public decimal CashDiscountPerUnit { get; set; }
        public decimal SpecialDiscountPerUnit { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Amount { get; set; }
        public int OrderedPrimaryQuantity { get; set; }
        public int OrderedQuantity { get; set; }
        public int OrderedPrimaryBonusQuantity { get; set; }
        public int OrderedBonusQuantity { get; set; }
        public int DeliveryPrimaryQuantity { get; set; }
        public int DeliveryQuantity { get; set; }
        public int DeliveryPrimaryBonusQuantity { get; set; }
        public decimal OtherDiscountPerUnit { get; set; }
        public decimal TransportationCostPerUnit { get; set; }
        public decimal DepoChargePerUnit { get; set; }
    }
}
