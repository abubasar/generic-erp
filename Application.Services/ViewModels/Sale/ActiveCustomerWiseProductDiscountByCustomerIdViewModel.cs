namespace Application.Services.ViewModels.Sale
{
    //customer discount
    public class ActiveCustomerWiseProductDiscountByCustomerIdViewModel
    {
        public Guid? ProductId { get; set; }
        public Guid? CustomerWiseProductDiscountId { get; set; }
        public decimal InvoiceDiscountPerUnit { get; set; }
        public decimal CashDiscountPerUnit { get; set; }
        public decimal SpecialDiscountPerUnit { get; set; }
    }
}
