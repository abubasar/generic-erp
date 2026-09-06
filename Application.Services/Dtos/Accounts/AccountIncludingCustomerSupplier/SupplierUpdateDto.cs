namespace Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier
{
    public class SupplierUpdateDto : SupplierCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedBankAccountIds { get; set; }
        public new ICollection<BankAccountUpdateDto>? BankAccounts { get; set; }
    }
}
