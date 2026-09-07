using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Services.Services.Platform;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers.Platform
{
    [Route("api/platform")]
    [ApiController]
    [PlatformAuthorize]
    public class PlatformCatalogController : ControllerBase
    {
        private readonly IPlatformCatalogService _catalog;

        public PlatformCatalogController(IPlatformCatalogService catalog) => _catalog = catalog;

        // ---- Modules ----

        [HttpGet("modules")]
        public async Task<Result> Modules() =>
            await Result<IReadOnlyList<ModuleDto>>.SuccessAsync(await _catalog.ListModulesAsync(), "OK");

        [HttpPost("modules")]
        [PlatformAuthorize(PlatformRoles.Admin)]
        public async Task<Result> UpsertModule(ModuleDto dto) =>
            await Result<ModuleDto>.SuccessAsync(await _catalog.UpsertModuleAsync(dto), "Saved");

        // ---- Business templates ----

        [HttpGet("business-templates")]
        public async Task<Result> Templates() =>
            await Result<IReadOnlyList<BusinessTemplateDto>>.SuccessAsync(await _catalog.ListTemplatesAsync(), "OK");

        [HttpPost("business-templates")]
        [PlatformAuthorize(PlatformRoles.Admin)]
        public async Task<Result> UpsertTemplate(BusinessTemplateDto dto) =>
            await Result<BusinessTemplateDto>.SuccessAsync(await _catalog.UpsertTemplateAsync(dto), "Saved");

        // ---- Plans ----

        [HttpGet("plans")]
        public async Task<Result> Plans() =>
            await Result<IReadOnlyList<PlanDto>>.SuccessAsync(await _catalog.ListPlansAsync(), "OK");

        [HttpPost("plans")]
        [PlatformAuthorize(PlatformRoles.Admin)]
        public async Task<Result> UpsertPlan(PlanDto dto) =>
            await Result<PlanDto>.SuccessAsync(await _catalog.UpsertPlanAsync(dto), "Saved");

        // ---- Price books ----

        [HttpGet("price-books")]
        public async Task<Result> PriceBooks() =>
            await Result<IReadOnlyList<PriceBookDto>>.SuccessAsync(await _catalog.ListPriceBooksAsync(), "OK");

        [HttpPost("price-books")]
        [PlatformAuthorize(PlatformRoles.Admin)]
        public async Task<Result> CreateDraft(CreatePriceBookDraftRequest request) =>
            await Result<PriceBookDto>.SuccessAsync(await _catalog.CreateDraftPriceBookAsync(request), "Draft created");

        [HttpPost("price-books/{id:guid}/publish")]
        [PlatformAuthorize(PlatformRoles.Admin)]
        public async Task<Result> Publish(Guid id) =>
            await Result<PriceBookDto>.SuccessAsync(await _catalog.PublishPriceBookAsync(id), "Published");

        // ---- Pricing quote ----

        [HttpPost("pricing/quote")]
        public async Task<Result> Quote([FromServices] IPricingEngine pricing, PricingRequest request) =>
            await Result<PricingQuote>.SuccessAsync(await pricing.QuoteAsync(request), "OK");
    }
}
