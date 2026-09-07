namespace Application.Core.Common
{
    /// <summary>
    /// Ambient current-tenant id for the executing request / async flow. Set once
    /// per request by <c>JwtMiddleware</c> from the JWT <c>tenantId</c> claim, and
    /// read by the <see cref="Data.DataContext"/> tenant query filters and by
    /// <c>UnitOfWork</c>'s save guard.
    ///
    /// Backed by <see cref="System.Threading.AsyncLocal{T}"/> so it flows with the
    /// request without threading a service through every constructor, and so EF Core
    /// can parameterise it per query. Outside a request (Hangfire jobs, design-time)
    /// it is <see cref="System.Guid.Empty"/>, which makes tenant-scoped queries
    /// return nothing rather than leak.
    /// </summary>
    public static class TenantScope
    {
        private static readonly AsyncLocal<Guid> Current = new();

        public static Guid CurrentTenantId
        {
            get => Current.Value;
            set => Current.Value = value;
        }

        public static bool HasTenant => Current.Value != Guid.Empty;
    }
}
