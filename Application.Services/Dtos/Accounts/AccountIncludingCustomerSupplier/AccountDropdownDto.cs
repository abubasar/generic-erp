namespace Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier
{
    public class AccountDropdownDto
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid AccountTypeId { get; set; }
        public string? TreeName { get; set; }
    }
}
