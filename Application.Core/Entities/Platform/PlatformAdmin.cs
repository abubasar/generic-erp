namespace Application.Core.Entities.Platform;

/// <summary>
/// A member of the platform operator's team. Lives ABOVE tenants — a platform
/// admin never holds tenant claims; it acts on a tenant through an explicit,
/// audited path. Authenticated by <c>PlatformAuthService</c> against a token that
/// carries no <c>TenantId</c>. See <c>docs/saas-platform-plan.md</c> §07.
/// </summary>
public class PlatformAdmin
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    /// <summary>Owner | Admin | Support | ReadOnly — coarse platform roles, checked by <c>[PlatformAuthorize]</c>.</summary>
    public string Role { get; set; } = "Admin";

    public byte[] PasswordHash { get; set; } = null!;

    public byte[] PasswordSalt { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginOn { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }
}
