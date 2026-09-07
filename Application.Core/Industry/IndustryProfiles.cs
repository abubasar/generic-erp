namespace Application.Core.Industry
{
    /// <summary>
    /// Maps a tenant's business-template / industry-profile key to its
    /// <see cref="IIndustryProfile"/>. The DI container resolves the per-request
    /// <see cref="IIndustryProfile"/> through here from
    /// <c>ITenantContext.BusinessTemplateKey</c> (template key == industry key
    /// today; add a template→industry lookup here if they ever diverge).
    /// </summary>
    public static class IndustryProfiles
    {
        private static readonly PharmacyProfile Pharmacy = new();
        private static readonly FeedProfile Feed = new();

        /// <summary>
        /// The profile for <paramref name="key"/>. Unknown / null keys fall back to
        /// <see cref="PharmacyProfile"/> (identity units) — the neutral choice that
        /// never silently rescales a quantity.
        /// </summary>
        public static IIndustryProfile For(string? key) => key?.Trim().ToLowerInvariant() switch
        {
            FeedProfile.ProfileKey => Feed,
            PharmacyProfile.ProfileKey => Pharmacy,
            _ => Pharmacy,
        };
    }
}
