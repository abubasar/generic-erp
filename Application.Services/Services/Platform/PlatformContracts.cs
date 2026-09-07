namespace Application.Services.Services.Platform
{
    // ---- Tenants ----

    public sealed record TenantListItem(
        Guid Id, string Code, string Name, string? Subdomain, string? BusinessTemplateKey,
        string Status, string? SubscriptionStatus, string? PlanKey, int ModuleCount, DateTime CreatedOn);

    public sealed record TenantDetail(
        Guid Id, string Code, string Name, string? Subdomain, string? CustomDomain,
        string? BusinessTemplateKey, string Status, string? Currency, int BusinessType,
        string? DbConnectionKey, DateTime CreatedOn,
        SubscriptionInfo? Subscription,
        IReadOnlyList<TenantModuleInfo> Modules,
        IReadOnlyList<QuotaInfo> Quotas);

    public sealed record SubscriptionInfo(Guid Id, string? PlanKey, string Status, DateTime PeriodStart, DateTime PeriodEnd, DateTime? TrialEndsOn);
    public sealed record TenantModuleInfo(string ModuleKey, string ModuleName, string Status, DateTime ActivatedOn, DateTime? ExpiresOn);
    public sealed record QuotaInfo(string Key, int Limit);

    public sealed record CreateTenantRequest(
        string Code, string Name, string BusinessTemplateKey, string? Subdomain,
        string? PlanKey, string? Currency, string? Email, string? ContactNo, string? Address);

    public sealed record SetSubscriptionRequest(string Status, string? PlanKey, DateTime? PeriodEnd, DateTime? TrialEndsOn);
    public sealed record ToggleModuleRequest(string ModuleKey, bool Enabled);
    public sealed record SetQuotaRequest(string Key, int Limit);
    public sealed record ImpersonateResult(string AccessToken, string RefreshToken, Guid TenantId, string TenantName, string ActingAs);

    // ---- Catalog ----

    public sealed record ModuleDto(string Key, string Name, string Category, string? Description, string? DependsOn, string? PermissionGroup, bool IsMetered, int SortOrder);
    public sealed record BusinessTemplateDto(string Key, string Name, string? Description, string DefaultModuleKeys, string IndustryProfileKey, bool IsPublic, int SortOrder);
    public sealed record PlanDto(string Key, string Name, string? Description, string ModuleKeys, string Quotas, bool IsPublic, bool IsActive, int SortOrder);

    public sealed record PriceBookDto(Guid Id, int Version, string Currency, string Status, DateTime? PublishedOn, string? Note, IReadOnlyList<PriceBookEntryDto> Entries);
    public sealed record PriceBookEntryDto(Guid Id, string ItemType, string ItemKey, decimal MonthlyPrice, decimal? UnitPrice);
    public sealed record UpsertPriceBookEntry(string ItemType, string ItemKey, decimal MonthlyPrice, decimal? UnitPrice);
    public sealed record CreatePriceBookDraftRequest(string Currency, string? Note, IReadOnlyList<UpsertPriceBookEntry> Entries);

    // ---- Usage ----

    public sealed record PlatformUsageSummary(
        int TenantsTotal, int TenantsActive, int TenantsTrial, int TenantsSuspended,
        decimal EstimatedMrr, string Currency,
        IReadOnlyList<ModuleAdoption> ModuleAdoption,
        IReadOnlyList<AuditEntryDto> RecentActivity);

    public sealed record ModuleAdoption(string ModuleKey, string ModuleName, int TenantCount);
    public sealed record AuditEntryDto(Guid Id, string AdminEmail, string Action, Guid? TenantId, string? Detail, DateTime CreatedOn);
}
