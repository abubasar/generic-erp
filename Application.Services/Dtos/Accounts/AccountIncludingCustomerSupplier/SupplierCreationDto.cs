namespace Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier
{
    public class SupplierCreationDto
    {
        public string? Name { get; set; }
        public string? OwnersName { get; set; }
        public string? ContactNo { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? NationalId { get; set; }
        public string? TradeLicense { get; set; }
        public string? ContactPersonName { get; set; }
        public string? ContactPersonContactNo { get; set; }
        public string? ContactPersonEmail { get; set; }
        public string? ContactPersonDesignation { get; set; }
        public IList<Guid>? SuppliedProductIds { get; set; }
        public virtual ICollection<BankAccountCreationDto>? BankAccounts { get; set; }
    }
}
