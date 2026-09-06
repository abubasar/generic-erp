namespace Application.Services.ViewModels.Accounts
{
    public class ControlAccount
    {
        public Guid? ParentId { get; set; }
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? TreeName { get; set; }
    }
}
