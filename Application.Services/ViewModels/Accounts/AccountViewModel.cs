namespace Application.Services.ViewModels.Accounts
{
    public class AccountViewModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public Guid AccountTypeId { get; set; }
        public string? AccountTypeName { get; set; }
        public int Level { get; set; }
        public string? Name { get; set; }
        public Guid ParentId { get; set; }
        public string? ParentName { get; set; }
        public bool IsControlAccount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
