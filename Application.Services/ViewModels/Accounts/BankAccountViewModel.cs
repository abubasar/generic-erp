namespace Application.Services.ViewModels.Accounts
{
    public class BankAccountViewModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? AccNo { get; set; }
        public string? RoutingNo { get; set; }
        public string? BankName { get; set; }
        public string? BranchName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
