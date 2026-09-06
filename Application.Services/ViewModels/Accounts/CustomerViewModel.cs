using Application.Core.Entities;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Accounts
{
    public class CustomerViewModel
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
        public Guid? CustomerRegionId { get; set; }
        public Guid? CustomerZoneId { get; set; }
        public Guid? CustomerAreaId { get; set; }
        public Guid? CustomerTerritoryId { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public decimal CustomerCreditLimit { get; set; }
        public bool CustomerAgreement { get; set; }
        public string? CustomerCreditDays { get; set; }
        public string? CustomerTargetQuantity { get; set; }
        public virtual ICollection<BankAccountViewModel>? BankAccounts { get; set; }
        public virtual EmployeeViewModel? CustomerMarketingOfficer { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
