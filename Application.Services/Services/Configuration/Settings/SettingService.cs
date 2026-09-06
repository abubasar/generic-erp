using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.Interfaces;

namespace Application.Services.Services.Configuration.Settings
{
    public class SettingService : ISettingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SettingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> FindKeyValue(string key)
        {
            var setting = await _unitOfWork.Repository<Setting>().FindAsync(x => x.Name == key);

            return setting.Value;
        }

    }
}
