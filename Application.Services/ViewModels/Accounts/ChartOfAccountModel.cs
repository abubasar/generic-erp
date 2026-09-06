namespace Application.Services.ViewModels.Accounts
{
    public class ChartOfAccountModel
    {
        public ChartOfAccountModel()
        {
            SubAccountHeads = new List<ChartOfAccountModel>();
        }
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public int Level { get; set; }
        public Guid? ParentId { get; set; }
        public List<ChartOfAccountModel> SubAccountHeads { get; set; }
    }
}
