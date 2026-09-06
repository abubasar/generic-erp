namespace Application.Services.ViewModels.Accounts
{
    public class AccountHeadWithBalanceViewModel
    {

        public Guid? Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public int Level { get; set; }
        public Guid? ParentId { get; set; }
        public decimal Balance { get; set; }
    }
}
