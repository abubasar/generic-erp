namespace Application.Core.Common
{
    public static class TenancyConstants
    {
        /// <summary>
        /// Sentinel <c>TenantId</c> for rows shared by every tenant. Currently only
        /// the standard chart-of-accounts skeleton (the ~13 roots + the named
        /// system account heads from <see cref="Application.Core.Constants.AccountHeadConstants"/>
        /// + the intermediate groups between them) — the accounting engine keys off
        /// those exact GUIDs, so they must resolve in every tenant's context.
        /// Deliberately NOT <see cref="System.Guid.Empty"/> (which means "no tenant").
        /// </summary>
        public static readonly System.Guid SystemTenantId = new("ffffffff-ffff-ffff-ffff-ffffffffffff");
    }
}
