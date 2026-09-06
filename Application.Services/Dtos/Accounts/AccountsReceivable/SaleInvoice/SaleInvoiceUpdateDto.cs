namespace Application.Services.Dtos.Accounts.AccountsReceivable.SaleInvoice
{
    public class SaleInvoiceUpdateDto : SaleInvoiceCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedSaleInvoiceDetailIds { get; set; }
        public new ICollection<SaleInvoiceDetailUpdateDto>? SaleInvoiceDetails { get; set; }
    }
}
