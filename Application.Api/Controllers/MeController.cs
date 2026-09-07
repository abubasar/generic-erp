using Application.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    /// <summary>
    /// What the current tenant + user can do: enabled modules, quotas,
    /// subscription state, industry template. The Angular shell builds its menu
    /// and route guards from this instead of a hardcoded list.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MeController : ControllerBase
    {
        private readonly ITenantContext _tenant;

        public MeController(ITenantContext tenant) => _tenant = tenant;

        [HttpGet]
        public ActionResult<MeResponse> Get()
        {
            if (!_tenant.HasTenant)
                return Unauthorized();

            int.TryParse(HttpContext.Items["BusinessType"]?.ToString(), out var businessType);

            return new MeResponse
            {
                TenantId = _tenant.TenantId,
                UserId = HttpContext.Items["UserId"]?.ToString(),
                UserName = HttpContext.Items["UserName"]?.ToString(),
                BusinessType = businessType,
                BusinessTemplateKey = _tenant.BusinessTemplateKey,
                SubscriptionStatus = _tenant.Status.ToString(),
                Modules = _tenant.Modules.OrderBy(m => m).ToArray(),
                Quotas = _tenant.Quotas,
            };
        }

        public sealed class MeResponse
        {
            public Guid TenantId { get; set; }
            public string? UserId { get; set; }
            public string? UserName { get; set; }
            public int BusinessType { get; set; }
            public string? BusinessTemplateKey { get; set; }
            public string SubscriptionStatus { get; set; } = "None";
            public string[] Modules { get; set; } = [];
            public IReadOnlyDictionary<string, int> Quotas { get; set; } = new Dictionary<string, int>();
        }
    }
}
