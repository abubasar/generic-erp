namespace Application.Services.ViewModels.Configuration
{
    public class GenericViewModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid? ProductTypeId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual ProductTypeViewModel? ProductType { get; set; }
    }
}
