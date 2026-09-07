using Application.Core.Data;
using Application.Core.Entities.Platform;
using Application.Core.Interfaces;

namespace Application.Services.Services.Platform
{
    public interface IPlatformAuditWriter
    {
        /// <summary>Queues a platform-audit row on the shared <see cref="DataContext"/>; the caller's SaveChanges persists it.</summary>
        void Add(string action, Guid? tenantId, string? detail);
    }

    public sealed class PlatformAuditWriter : IPlatformAuditWriter
    {
        private readonly DataContext _db;
        private readonly IPlatformAdminContext _admin;

        public PlatformAuditWriter(DataContext db, IPlatformAdminContext admin)
        {
            _db = db;
            _admin = admin;
        }

        public void Add(string action, Guid? tenantId, string? detail) =>
            _db.PlatformAuditLogs.Add(new PlatformAuditLog
            {
                Id = Guid.NewGuid(),
                PlatformAdminId = _admin.AdminId,
                PlatformAdminEmail = _admin.Email ?? "system",
                Action = action,
                TenantId = tenantId,
                Detail = detail is { Length: > 4000 } ? detail[..4000] : detail,
                CreatedOn = DateTime.UtcNow,
            });
    }
}
