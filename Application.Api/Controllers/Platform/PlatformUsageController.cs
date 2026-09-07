using Application.Api.Attributes;
using Application.Core.Common;
using Application.Services.Services.Platform;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers.Platform
{
    [Route("api/platform")]
    [ApiController]
    [PlatformAuthorize]
    public class PlatformUsageController : ControllerBase
    {
        private readonly IPlatformUsageService _usage;

        public PlatformUsageController(IPlatformUsageService usage) => _usage = usage;

        [HttpGet("usage")]
        public async Task<Result> Summary() =>
            await Result<PlatformUsageSummary>.SuccessAsync(await _usage.GetSummaryAsync(), "OK");

        [HttpGet("audit")]
        public async Task<Result> Audit([FromQuery] Guid? tenantId, [FromQuery] int take = 100) =>
            await Result<IReadOnlyList<AuditEntryDto>>.SuccessAsync(await _usage.GetAuditAsync(tenantId, take), "OK");
    }
}
