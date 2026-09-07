using Application.Core.Constants;
using Application.Core.Data;
using Application.Core.Entities;
using Application.Core.Entities.Platform;
using Application.Core.Exceptions;
using Application.Core.PermissionHelpers;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Platform
{
    public interface IProvisioningService
    {
        /// <summary>Runs (or re-runs) every provisioning step for a tenant. Idempotent.</summary>
        Task<ProvisioningResult> ProvisionAsync(Guid tenantId, ProvisionRequest request);

        Task<IReadOnlyList<ProvisioningStepStatus>> GetStatusAsync(Guid tenantId);
    }

    public sealed class ProvisioningService : IProvisioningService
    {
        private readonly DataContext _db;
        private readonly IPlatformAuditWriter _audit;

        public ProvisioningService(DataContext db, IPlatformAuditWriter audit)
        {
            _db = db;
            _audit = audit;
        }

        private static readonly string[] BaseUnits =
            { "Piece", "Kilogram", "Gram", "Litre", "Millilitre", "Bag", "Carton", "Dozen" };

        public async Task<ProvisioningResult> ProvisionAsync(Guid tenantId, ProvisionRequest request)
        {
            var tenant = await _db.Set<Tenant>().IgnoreQueryFilters().FirstOrDefaultAsync(t => t.Id == tenantId && !t.Deleted)
                ?? throw new BadRequestException("Tenant not found.");

            string? username = null, tempPassword = null;

            await RunStep(tenantId, "subdomain", () => EnsureSubdomain(tenant));
            await RunStep(tenantId, "subscription", () => EnsureSubscription(tenantId));
            await RunStep(tenantId, "financial-year", () => EnsureFinancialYear(tenantId));
            await RunStep(tenantId, "measurement-units", () => EnsureUnits(tenantId));
            await RunStep(tenantId, "company", () => EnsureCompany(tenant));
            await RunStep(tenantId, "default-store", () => EnsureStore(tenantId));
            var roleId = Guid.Empty;
            await RunStep(tenantId, "owner-role", async () => roleId = await EnsureOwnerRole(tenant));
            await RunStep(tenantId, "owner-user", async () =>
            {
                (username, tempPassword) = await EnsureOwnerUser(tenant, request, roleId);
            });

            await _db.SaveChangesAsync();

            var steps = await GetStatusAsync(tenantId);
            var complete = steps.All(s => s.Status == "Done");
            _audit.Add("tenant.provision", tenantId, $"complete={complete}; steps={string.Join(',', steps.Where(s => s.Status != "Done").Select(s => s.StepKey))}");
            await _db.SaveChangesAsync();

            // If owner-user was already Done on a prior run we won't have a fresh password.
            return new ProvisioningResult(tenantId, complete, username ?? await ExistingOwnerUsername(tenantId, roleId), tempPassword, steps);
        }

        public async Task<IReadOnlyList<ProvisioningStepStatus>> GetStatusAsync(Guid tenantId) =>
            await _db.ProvisioningSteps.Where(s => s.TenantId == tenantId).OrderBy(s => s.StartedOn ?? DateTime.MaxValue)
                .Select(s => new ProvisioningStepStatus(s.StepKey, s.Status, s.Error, s.Attempts, s.CompletedOn))
                .ToListAsync();

        // ---- step runner ----

        private async Task RunStep(Guid tenantId, string key, Func<Task> work)
        {
            var step = await _db.ProvisioningSteps.FirstOrDefaultAsync(s => s.TenantId == tenantId && s.StepKey == key);
            if (step is null)
            {
                step = new ProvisioningStep { Id = Guid.NewGuid(), TenantId = tenantId, StepKey = key, Status = "Pending" };
                _db.ProvisioningSteps.Add(step);
            }
            if (step.Status == "Done") return;

            step.Attempts++;
            step.StartedOn = DateTime.UtcNow;
            try
            {
                await work();
                await _db.SaveChangesAsync();
                step.Status = "Done";
                step.Error = null;
                step.CompletedOn = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                step.Status = "Failed";
                step.Error = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
            }
            await _db.SaveChangesAsync();
            if (step.Status == "Failed")
                throw new BadRequestException($"Provisioning step '{key}' failed: {step.Error}");
        }

        private Task RunStep(Guid tenantId, string key, Action work) => RunStep(tenantId, key, () => { work(); return Task.CompletedTask; });

        // ---- steps ----

        private async Task EnsureSubdomain(Tenant tenant)
        {
            if (!string.IsNullOrWhiteSpace(tenant.Subdomain)) return;
            var baseSlug = Slug(string.IsNullOrWhiteSpace(tenant.Code) ? tenant.Name : tenant.Code);
            var slug = baseSlug;
            var n = 1;
            while (await _db.Set<Tenant>().IgnoreQueryFilters().AnyAsync(t => t.Subdomain == slug && t.Id != tenant.Id))
                slug = $"{baseSlug}{++n}";
            tenant.Subdomain = slug;
            tenant.UpdatedOn = DateTime.UtcNow;
        }

        private async Task EnsureSubscription(Guid tenantId)
        {
            if (await _db.Subscriptions.AnyAsync(s => s.TenantId == tenantId)) return;
            var now = DateTime.UtcNow;
            _db.Subscriptions.Add(new Subscription
            {
                Id = Guid.NewGuid(), TenantId = tenantId, Status = "Trial",
                PeriodStart = now, PeriodEnd = now.AddYears(1), TrialEndsOn = now.AddDays(14),
            });
        }

        private async Task EnsureFinancialYear(Guid tenantId)
        {
            if (await _db.Set<FinancialYear>().IgnoreQueryFilters().AnyAsync(f => f.TenantId == tenantId && f.IsActive && !f.Deleted))
                return;

            var today = DateTime.UtcNow.Date;
            // Bangladesh fiscal year: 1 July – 30 June.
            var startYear = today.Month >= 7 ? today.Year : today.Year - 1;
            var start = new DateTime(startYear, 7, 1);
            var end = new DateTime(startYear + 1, 6, 30);
            var name = $"{startYear}-{startYear + 1}";
            var code = $"{startYear % 100:D2}{(startYear + 1) % 100:D2}";

            _db.Set<FinancialYear>().Add(new FinancialYear
            {
                Id = Guid.NewGuid(), TenantId = tenantId, Name = name, Code = code,
                StartDate = start, EndDate = end, IsActive = true, Deleted = false,
                CreatedOn = DateTime.UtcNow, UpdatedOn = DateTime.UtcNow, CreatedBy = "provisioning", UpdatedBy = "provisioning",
            });
        }

        private async Task EnsureUnits(Guid tenantId)
        {
            var existing = await _db.Set<MeasurementUnit>().IgnoreQueryFilters()
                .Where(u => u.TenantId == tenantId && !u.Deleted).Select(u => u.Name).ToListAsync();
            foreach (var name in BaseUnits.Where(n => !existing.Contains(n)))
            {
                _db.Set<MeasurementUnit>().Add(new MeasurementUnit
                {
                    Id = Guid.NewGuid(), TenantId = tenantId, Name = name, Deleted = false,
                    CreatedOn = DateTime.UtcNow, UpdatedOn = DateTime.UtcNow, CreatedBy = "provisioning", UpdatedBy = "provisioning",
                });
            }
        }

        private async Task EnsureCompany(Tenant tenant)
        {
            if (await _db.Set<Company>().IgnoreQueryFilters().AnyAsync(c => c.TenantId == tenant.Id && !c.Deleted)) return;
            _db.Set<Company>().Add(new Company
            {
                Id = Guid.NewGuid(), TenantId = tenant.Id, Code = "MAIN", Name = tenant.Name, Deleted = false,
                CreatedOn = DateTime.UtcNow, UpdatedOn = DateTime.UtcNow, CreatedBy = "provisioning", UpdatedBy = "provisioning",
            });
        }

        private async Task EnsureStore(Guid tenantId)
        {
            if (await _db.Set<Store>().IgnoreQueryFilters().AnyAsync(s => s.TenantId == tenantId && !s.Deleted)) return;
            _db.Set<Store>().Add(new Store
            {
                Id = Guid.NewGuid(), TenantId = tenantId, Code = "MAIN", Name = "Main Store",
                InventoryTypeId = Guid.Parse(InventoryTypeConstants.Inventory_Type_Id_Finished_Goods),
                DepoChargePerKg = 0m, Deleted = false,
                CreatedOn = DateTime.UtcNow, UpdatedOn = DateTime.UtcNow, CreatedBy = "provisioning", UpdatedBy = "provisioning",
            });
        }

        private async Task<Guid> EnsureOwnerRole(Tenant tenant)
        {
            var role = await _db.Set<Role>().IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.TenantId == tenant.Id && r.Name == "Owner" && !r.Deleted);
            if (role is null)
            {
                role = new Role
                {
                    Id = Guid.NewGuid(), TenantId = tenant.Id, Name = "Owner", Deleted = false,
                    CreatedOn = DateTime.UtcNow, UpdatedOn = DateTime.UtcNow, CreatedBy = "provisioning", UpdatedBy = "provisioning",
                };
                _db.Set<Role>().Add(role);
            }

            var have = await _db.Set<RoleClaim>().IgnoreQueryFilters()
                .Where(c => c.RoleId == role.Id).Select(c => c.Value).ToListAsync();

            var all = new List<RoleClaimModel>();
            all.GetAllPermissions(tenant.BusinessType);
            foreach (var perm in all.Where(p => p.Value is not null && !have.Contains(p.Value)).DistinctBy(p => p.Value))
            {
                _db.Set<RoleClaim>().Add(new RoleClaim
                {
                    Id = Guid.NewGuid(), TenantId = tenant.Id, RoleId = role.Id,
                    Type = ApplicationClaimTypes.Permission, Value = perm.Value!,
                    Group = perm.Group ?? "", Description = perm.Description ?? "",
                    Deleted = false, CreatedBy = "provisioning", UpdatedBy = "provisioning",
                });
            }
            return role.Id;
        }

        private async Task<(string username, string? tempPassword)> EnsureOwnerUser(Tenant tenant, ProvisionRequest request, Guid roleId)
        {
            if (roleId == Guid.Empty)
                roleId = await _db.Set<Role>().IgnoreQueryFilters()
                    .Where(r => r.TenantId == tenant.Id && r.Name == "Owner" && !r.Deleted).Select(r => r.Id).FirstAsync();

            var username = (request.OwnerUsername ?? request.OwnerEmail ?? $"owner@{tenant.Subdomain}").Trim().ToLowerInvariant();

            var existing = await _db.Set<User>().IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.Username == username && !u.Deleted);
            if (existing is not null)
                return (existing.Username, null);

            var password = string.IsNullOrWhiteSpace(request.OwnerPassword) ? GeneratePassword() : request.OwnerPassword;
            var (hash, salt) = PlatformPasswordHasher.Create(password);

            _db.Set<User>().Add(new User
            {
                Id = Guid.NewGuid(), TenantId = tenant.Id, Username = username, RoleId = roleId,
                PasswordHash = hash, PasswordSalt = salt, Deleted = false,
                CreatedOn = DateTime.UtcNow, UpdatedOn = DateTime.UtcNow, CreatedBy = "provisioning", UpdatedBy = "provisioning",
            });
            return (username, string.IsNullOrWhiteSpace(request.OwnerPassword) ? password : null);
        }

        private async Task<string?> ExistingOwnerUsername(Guid tenantId, Guid roleId)
        {
            var q = _db.Set<User>().IgnoreQueryFilters().Where(u => u.TenantId == tenantId && !u.Deleted);
            if (roleId != Guid.Empty) q = q.Where(u => u.RoleId == roleId);
            return await q.Select(u => u.Username).FirstOrDefaultAsync();
        }

        // ---- helpers ----

        private static string Slug(string s)
        {
            var chars = s.Trim().ToLowerInvariant()
                .Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray();
            var slug = new string(chars).Trim('-');
            return string.IsNullOrEmpty(slug) ? "tenant" : slug[..Math.Min(slug.Length, 40)];
        }

        private static string GeneratePassword()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(14);
            var chars = bytes.Select(b => alphabet[b % alphabet.Length]).ToArray();
            return "Bt" + new string(chars) + "!9";
        }
    }
}
