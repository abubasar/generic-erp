
namespace Application.Services.Services.Configuration.Settings
{
    public interface ISettingService
    {
        Task<string> FindKeyValue(string name);
    }
}
