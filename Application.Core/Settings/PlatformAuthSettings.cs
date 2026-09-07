namespace Application.Core.Settings
{
    /// <summary>
    /// Auth config for the platform-admin console. Deliberately separate from
    /// <see cref="JwtSettings"/> (the tenant token) — different audience so a
    /// tenant token can never be presented as a platform token, and vice versa.
    /// </summary>
    public class PlatformAuthSettings
    {
        public string Secret { get; set; } = "";
        public string Issuer { get; set; } = "https://platform.butsbd.com";
        public string Audience { get; set; } = "https://platform.butsbd.com";
        public int ExpiryMinutes { get; set; } = 480;

        /// <summary>First-run seed: if no PlatformAdmin exists, one is created from these.</summary>
        public string SeedEmail { get; set; } = "";
        public string SeedPassword { get; set; } = "";
        public string SeedName { get; set; } = "Platform Owner";
    }
}
