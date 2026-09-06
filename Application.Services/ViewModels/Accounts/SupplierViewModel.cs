namespace Application.Services.ViewModels.Accounts
{
    public class SupplierViewModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
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
        public virtual ICollection<BankAccountViewModel>? BankAccounts { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
