namespace Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier
{
    public class BankAccountCreationDto
    {
        public string? Name { get; set; }
        public string? AccNo { get; set; }
        public string? RoutingNo { get; set; }
        public string? BankName { get; set; }
        public string? BranchName { get; set; }
    }
}
