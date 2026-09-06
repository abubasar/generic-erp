namespace Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier
{
    public class AccountCreationDto
    {
        public string? Code { get; set; }
        public Guid AccountTypeId { get; set; }
        public int Level { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ParentId { get; set; }
        public bool IsControlAccount { get; set; }
    }
}
