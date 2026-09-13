using Application.Core.Common;
using Application.Core.Exceptions;
using Application.Services.Services.Platform;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    /// <summary>
    /// The anonymous self-service signup surface (docs/saas-platform-plan.md §06) —
    /// deliberately separate from <c>Controllers/Platform/*</c>, which is entirely
    /// gated behind <c>[PlatformAuthorize]</c>. No auth attribute here: any visitor
    /// can list the public catalog, get a quote, and create their own tenant. Only
    /// templates/plans marked IsPublic (and, for plans, IsActive) are reachable.
    /// </summary>
    [Route("api/signup")]
    [ApiController]
    public class SignupController : ControllerBase
    {
        private readonly IPlatformCatalogService _catalog;
        private readonly IPlatformTenantService _tenants;
        private readonly IPricingEngine _pricing;

        public SignupController(IPlatformCatalogService catalog, IPlatformTenantService tenants, IPricingEngine pricing)
        {
            _catalog = catalog;
            _tenants = tenants;
            _pricing = pricing;
        }

        [HttpGet("business-templates")]
        public async Task<Result> BusinessTemplates() =>
            await Result<IReadOnlyList<BusinessTemplateDto>>.SuccessAsync(await _catalog.ListPublicTemplatesAsync(), "OK");

        [HttpGet("plans")]
        public async Task<Result> Plans() =>
            await Result<IReadOnlyList<PlanDto>>.SuccessAsync(await _catalog.ListPublicPlansAsync(), "OK");

        [HttpPost("pricing/quote")]
        public async Task<Result> Quote(PricingRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.PlanKey))
            {
                var plans = await _catalog.ListPublicPlansAsync();
                if (!plans.Any(p => p.Key == request.PlanKey))
                    throw new BadRequestException($"Plan '{request.PlanKey}' is not available for signup.");
            }
            return await Result<PricingQuote>.SuccessAsync(await _pricing.QuoteAsync(request), "OK");
        }

        [HttpPost("tenants")]
        public async Task<Result> SignUp(PublicSignupRequest request) =>
            await Result<CreateTenantResult>.SuccessAsync(await _tenants.SignUpAsync(request), "Welcome aboard");
    }
}
