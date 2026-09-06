namespace Application.Services.ViewModels.Accounts
{
    public class AccountTypeViewModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public int StartingNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
