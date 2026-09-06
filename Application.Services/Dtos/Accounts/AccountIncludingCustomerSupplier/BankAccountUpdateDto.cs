namespace Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier
{
    public class BankAccountUpdateDto : BankAccountCreationDto
    {
        public Guid? Id { get; set; }
    }
}
