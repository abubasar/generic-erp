using Application.Core.Common;
using Application.Services.Services.Configuration.Settings;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingController : ControllerBase
    {
        public readonly ISettingService _settingService;

        public SettingController(ISettingService settingService)
        {
            _settingService = settingService;
        }


        [Route("{name}")]
        [HttpGet]
        public virtual async Task<Result> GetSettingValue(string name)
        {
            var settingValue = await _settingService.FindKeyValue(name);

            return await Result<string>.SuccessAsync(settingValue, "Result Found");
        }
    }
}
