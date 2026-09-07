using Application.Core.Exceptions;
using Application.Core.Data;
using Application.Core.Entities.Platform;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Platform
{
    public interface IPlatformCatalogService
    {
        Task<IReadOnlyList<ModuleDto>> ListModulesAsync();
        Task<ModuleDto> UpsertModuleAsync(ModuleDto dto);

        Task<IReadOnlyList<BusinessTemplateDto>> ListTemplatesAsync();
        Task<BusinessTemplateDto> UpsertTemplateAsync(BusinessTemplateDto dto);

        Task<IReadOnlyList<PlanDto>> ListPlansAsync();
        Task<PlanDto> UpsertPlanAsync(PlanDto dto);

        Task<IReadOnlyList<PriceBookDto>> ListPriceBooksAsync();
        Task<PriceBookDto> CreateDraftPriceBookAsync(CreatePriceBookDraftRequest request);
        Task<PriceBookDto> PublishPriceBookAsync(Guid priceBookId);
    }

    public sealed class PlatformCatalogService : IPlatformCatalogService
    {
        private readonly DataContext _db;
        private readonly IPlatformAuditWriter _audit;

        public PlatformCatalogService(DataContext db, IPlatformAuditWriter audit)
        {
            _db = db;
            _audit = audit;
        }

        // ---- Modules ----

        public async Task<IReadOnlyList<ModuleDto>> ListModulesAsync() =>
            await _db.Modules.OrderBy(m => m.SortOrder).ThenBy(m => m.Key)
                .Select(m => new ModuleDto(m.Key, m.Name, m.Category, m.Description, m.DependsOn, m.PermissionGroup, m.IsMetered, m.SortOrder))
                .ToListAsync();

        public async Task<ModuleDto> UpsertModuleAsync(ModuleDto dto)
        {
            var key = dto.Key.Trim().ToLowerInvariant();
            var m = await _db.Modules.FirstOrDefaultAsync(x => x.Key == key);
            var isNew = m is null;
            m ??= new PlatformModule { Key = key };
            m.Name = dto.Name.Trim();
            m.Category = string.IsNullOrWhiteSpace(dto.Category) ? "Business" : dto.Category.Trim();
            m.Description = dto.Description;
            m.DependsOn = string.IsNullOrWhiteSpace(dto.DependsOn) ? null : dto.DependsOn.Trim();
            m.PermissionGroup = dto.PermissionGroup;
            m.IsMetered = dto.IsMetered;
            m.SortOrder = dto.SortOrder;
            if (isNew) _db.Modules.Add(m);

            _audit.Add(isNew ? "module.create" : "module.update", null, $"key={key}");
            await _db.SaveChangesAsync();
            return new ModuleDto(m.Key, m.Name, m.Category, m.Description, m.DependsOn, m.PermissionGroup, m.IsMetered, m.SortOrder);
        }

        // ---- Business templates ----

        public async Task<IReadOnlyList<BusinessTemplateDto>> ListTemplatesAsync() =>
            await _db.BusinessTemplates.OrderBy(b => b.SortOrder).ThenBy(b => b.Key)
                .Select(b => new BusinessTemplateDto(b.Key, b.Name, b.Description, b.DefaultModuleKeys, b.IndustryProfileKey, b.IsPublic, b.SortOrder))
                .ToListAsync();

        public async Task<BusinessTemplateDto> UpsertTemplateAsync(BusinessTemplateDto dto)
        {
            var key = dto.Key.Trim().ToLowerInvariant();
            var b = await _db.BusinessTemplates.FirstOrDefaultAsync(x => x.Key == key);
            var isNew = b is null;
            b ??= new BusinessTemplate { Key = key };
            b.Name = dto.Name.Trim();
            b.Description = dto.Description;
            b.DefaultModuleKeys = NormalizeKeys(dto.DefaultModuleKeys);
            b.IndustryProfileKey = string.IsNullOrWhiteSpace(dto.IndustryProfileKey) ? key : dto.IndustryProfileKey.Trim().ToLowerInvariant();
            b.IsPublic = dto.IsPublic;
            b.SortOrder = dto.SortOrder;
            if (isNew) _db.BusinessTemplates.Add(b);

            _audit.Add(isNew ? "template.create" : "template.update", null, $"key={key}");
            await _db.SaveChangesAsync();
            return new BusinessTemplateDto(b.Key, b.Name, b.Description, b.DefaultModuleKeys, b.IndustryProfileKey, b.IsPublic, b.SortOrder);
        }

        // ---- Plans ----

        public async Task<IReadOnlyList<PlanDto>> ListPlansAsync() =>
            await _db.Plans.OrderBy(p => p.SortOrder).ThenBy(p => p.Key)
                .Select(p => new PlanDto(p.Key, p.Name, p.Description, p.ModuleKeys, p.Quotas, p.IsPublic, p.IsActive, p.SortOrder))
                .ToListAsync();

        public async Task<PlanDto> UpsertPlanAsync(PlanDto dto)
        {
            var key = dto.Key.Trim().ToLowerInvariant();
            var p = await _db.Plans.FirstOrDefaultAsync(x => x.Key == key);
            var isNew = p is null;
            p ??= new Plan { Key = key };
            p.Name = dto.Name.Trim();
            p.Description = dto.Description;
            p.ModuleKeys = NormalizeKeys(dto.ModuleKeys);
            p.Quotas = (dto.Quotas ?? "").Trim();
            p.IsPublic = dto.IsPublic;
            p.IsActive = dto.IsActive;
            p.SortOrder = dto.SortOrder;
            if (isNew) _db.Plans.Add(p);

            _audit.Add(isNew ? "plan.create" : "plan.update", null, $"key={key}");
            await _db.SaveChangesAsync();
            return new PlanDto(p.Key, p.Name, p.Description, p.ModuleKeys, p.Quotas, p.IsPublic, p.IsActive, p.SortOrder);
        }

        // ---- Price books ----

        public async Task<IReadOnlyList<PriceBookDto>> ListPriceBooksAsync()
        {
            var books = await _db.PriceBooks.Include(b => b.Entries).OrderByDescending(b => b.Version).ToListAsync();
            return books.Select(ToDto).ToList();
        }

        public async Task<PriceBookDto> CreateDraftPriceBookAsync(CreatePriceBookDraftRequest request)
        {
            var nextVersion = (await _db.PriceBooks.MaxAsync(b => (int?)b.Version) ?? 0) + 1;
            var now = DateTime.UtcNow;
            var book = new PriceBook
            {
                Id = Guid.NewGuid(),
                Version = nextVersion,
                Currency = string.IsNullOrWhiteSpace(request.Currency) ? "BDT" : request.Currency.Trim().ToUpperInvariant(),
                Status = "Draft",
                Note = request.Note,
                CreatedOn = now,
                UpdatedOn = now,
                Entries = request.Entries.Select(e => new PriceBookEntry
                {
                    Id = Guid.NewGuid(),
                    ItemType = e.ItemType.Trim().ToLowerInvariant(),
                    ItemKey = e.ItemKey.Trim().ToLowerInvariant(),
                    MonthlyPrice = e.MonthlyPrice,
                    UnitPrice = e.UnitPrice,
                }).ToList(),
            };
            _db.PriceBooks.Add(book);

            _audit.Add("pricebook.draft", null, $"version={nextVersion}; entries={book.Entries.Count}");
            await _db.SaveChangesAsync();
            return ToDto(book);
        }

        public async Task<PriceBookDto> PublishPriceBookAsync(Guid priceBookId)
        {
            var book = await _db.PriceBooks.Include(b => b.Entries).FirstOrDefaultAsync(b => b.Id == priceBookId)
                ?? throw new BadRequestException("Price book not found.");
            if (book.Status == "Published") return ToDto(book);

            foreach (var live in await _db.PriceBooks.Where(b => b.Status == "Published").ToListAsync())
                live.Status = "Archived";

            book.Status = "Published";
            book.PublishedOn = DateTime.UtcNow;
            book.UpdatedOn = DateTime.UtcNow;

            _audit.Add("pricebook.publish", null, $"version={book.Version}");
            await _db.SaveChangesAsync();
            return ToDto(book);
        }

        private static PriceBookDto ToDto(PriceBook b) => new(
            b.Id, b.Version, b.Currency, b.Status, b.PublishedOn, b.Note,
            b.Entries.OrderBy(e => e.ItemType).ThenBy(e => e.ItemKey)
                .Select(e => new PriceBookEntryDto(e.Id, e.ItemType, e.ItemKey, e.MonthlyPrice, e.UnitPrice)).ToList());

        private static string NormalizeKeys(string? csv) => string.Join(',',
            (csv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(k => k.ToLowerInvariant()).Distinct());
    }
}
