using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Exceptions;
using Application.Services.Services.Platform;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers.Platform
{
    [Route("api/platform/tenants")]
    [ApiController]
    [PlatformAuthorize]
    public class PlatformTenantsController : ControllerBase
    {
        private readonly IPlatformTenantService _tenants;
        private readonly IProvisioningService _provisioning;
        private readonly IPlatformInvoiceService _invoices;

        public PlatformTenantsController(IPlatformTenantService tenants, IProvisioningService provisioning, IPlatformInvoiceService invoices)
        {
            _tenants = tenants;
            _provisioning = provisioning;
            _invoices = invoices;
        }

        [HttpGet]
        public async Task<Result> List([FromQuery] string? search, [FromQuery] string? status) =>
            await Result<IReadOnlyList<TenantListItem>>.SuccessAsync(await _tenants.ListAsync(search, status), "OK");

        [HttpGet("{id:guid}")]
        public async Task<Result> Get(Guid id)
        {
            var detail = await _tenants.GetAsync(id) ?? throw new NotFoundResultException("Tenant not found.");
            return await Result<TenantDetail>.SuccessAsync(detail, "OK");
        }

        [HttpPost]
        [PlatformAuthorize(PlatformRoles.Admin)]
        public async Task<Result> Create(CreateTenantRequest request) =>
            await Result<CreateTenantResult>.SuccessAsync(await _tenants.CreateAsync(request), "Tenant created");

        [HttpPost("{id:guid}/provision")]
        [PlatformAuthorize(PlatformRoles.Admin)]
        public async Task<Result> Provision(Guid id, ProvisionRequest request) =>
            await Result<ProvisioningResult>.SuccessAsync(await _provisioning.ProvisionAsync(id, request), "Provisioning run");

        [HttpGet("{id:guid}/provisioning")]
        public async Task<Result> ProvisioningStatus(Guid id) =>
            await Result<IReadOnlyList<ProvisioningStepStatus>>.SuccessAsync(await _provisioning.GetStatusAsync(id), "OK");

        [HttpPost("{id:guid}/status")]
        [PlatformAuthorize(PlatformRoles.Admin)]
        public async Task<Result> SetStatus(Guid id, [FromBody] StatusBody body) =>
            await Result<TenantDetail>.SuccessAsync(await _tenants.SetStatusAsync(id, body.Status), "Status updated");

        [HttpPost("{id:guid}/subscription")]
        [PlatformAuthorize(PlatformRoles.Admin)]
        public async Task<Result> SetSubscription(Guid id, SetSubscriptionRequest request) =>
            await Result<TenantDetail>.SuccessAsync(await _tenants.SetSubscriptionAsync(id, request), "Subscription updated");

        [HttpPost("{id:guid}/modules")]
        [PlatformAuthorize(PlatformRoles.Admin)]
        public async Task<Result> ToggleModule(Guid id, ToggleModuleRequest request) =>
            await Result<TenantDetail>.SuccessAsync(await _tenants.ToggleModuleAsync(id, request), "Module updated");

        [HttpPost("{id:guid}/quotas")]
        [PlatformAuthorize(PlatformRoles.Admin)]
        public async Task<Result> SetQuota(Guid id, SetQuotaRequest request) =>
            await Result<TenantDetail>.SuccessAsync(await _tenants.SetQuotaAsync(id, request), "Quota updated");

        [HttpPost("{id:guid}/impersonate")]
        [PlatformAuthorize(PlatformRoles.Support)]
        public async Task<Result> Impersonate(Guid id) =>
            await Result<ImpersonateResult>.SuccessAsync(await _tenants.ImpersonateAsync(id), "Impersonation token issued");

        // ---- Manual invoicing (docs/saas-platform-plan.md §08 Phase 2) ----

        [HttpGet("{id:guid}/invoices")]
        public async Task<Result> Invoices(Guid id) =>
            await Result<IReadOnlyList<PlatformInvoiceDto>>.SuccessAsync(await _invoices.ListAsync(id), "OK");

        [HttpPost("{id:guid}/invoices")]
        [PlatformAuthorize(PlatformRoles.Admin)]
        public async Task<Result> CreateInvoice(Guid id, CreateInvoiceRequest request) =>
            await Result<PlatformInvoiceDto>.SuccessAsync(await _invoices.CreateAsync(id, request), "Invoice created");

        [HttpPost("{id:guid}/invoices/{invoiceId:guid}/paid")]
        [PlatformAuthorize(PlatformRoles.Admin)]
        public async Task<Result> MarkInvoicePaid(Guid id, Guid invoiceId) =>
            await Result<PlatformInvoiceDto>.SuccessAsync(await _invoices.MarkPaidAsync(invoiceId), "Invoice marked paid");

        [HttpPost("{id:guid}/invoices/{invoiceId:guid}/void")]
        [PlatformAuthorize(PlatformRoles.Admin)]
        public async Task<Result> VoidInvoice(Guid id, Guid invoiceId) =>
            await Result<PlatformInvoiceDto>.SuccessAsync(await _invoices.VoidAsync(invoiceId), "Invoice voided");

        public sealed record StatusBody(string Status);
    }
}
