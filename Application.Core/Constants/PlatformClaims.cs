namespace Application.Core.Constants
{
    /// <summary>Claim types on a platform-admin token. Note: NO tenant claim ever.</summary>
    public static class PlatformClaims
    {
        public const string AdminId = "pid";
        public const string Email = "pemail";
        public const string Name = "pname";
        public const string Role = "prole";

        /// <summary>Marks the token as a platform token; the middleware rejects anything else on /api/platform/*.</summary>
        public const string Scope = "scope";
        public const string ScopeValue = "platform";
    }

    /// <summary>Coarse platform-admin roles, most-privileged first.</summary>
    public static class PlatformRoles
    {
        public const string Owner = "Owner";
        public const string Admin = "Admin";
        public const string Support = "Support";
        public const string ReadOnly = "ReadOnly";

        public static readonly string[] All = { Owner, Admin, Support, ReadOnly };

        /// <summary>True when <paramref name="role"/> is at least as privileged as <paramref name="required"/>.</summary>
        public static bool Satisfies(string? role, string required)
        {
            int Rank(string r) => r switch
            {
                Owner => 3,
                Admin => 2,
                Support => 1,
                ReadOnly => 0,
                _ => -1,
            };
            return Rank(role ?? "") >= Rank(required);
        }
    }
}
