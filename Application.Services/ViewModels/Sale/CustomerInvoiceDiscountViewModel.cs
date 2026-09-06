namespace Application.Services.ViewModels.Sale
{
    public class CustomerInvoiceDiscountViewModel
    {
        public Guid? CustomerWiseProductDiscountId { get; set; }
        public decimal InvoiceDiscountPerUnit { get; set; }
        public decimal CashDiscountPerUnit { get; set; }
        public decimal SpecialDiscountPerUnit { get; set; }
    }
}
