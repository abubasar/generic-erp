using Application.Services.ViewModels.Accounts;

namespace Application.Services.ViewModels.Configuration
{
    public class CustomerWiseProductDiscountViewModel
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime ApplicableDate { get; set; }
        public bool IsActive { get; set; }
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public string? CheckedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual CustomerViewModel? Customer { get; set; }
        public virtual ICollection<CustomerWiseProductDiscountDetailViewModel>? CustomerWiseProductDiscountDetails { get; set; }
    }
}
