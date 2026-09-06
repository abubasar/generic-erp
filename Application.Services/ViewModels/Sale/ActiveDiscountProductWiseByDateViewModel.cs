namespace Application.Services.ViewModels.Sale
{
    //Offer Discount
    public class ActiveDiscountProductWiseByDateViewModel
    {
        public Guid? DiscountProductWiseId { get; set; }
        public Guid? ProductId { get; set; }
        public decimal DiscountAmountPerKg { get; set; }
    }
}
