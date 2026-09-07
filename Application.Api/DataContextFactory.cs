using Application.Core.Data;
using Application.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Application.Api
{
    /// <summary>
    /// Design-time factory for <c>dotnet ef</c> (migrations / scaffolding).
    /// The runtime <see cref="DataContext"/> has two constructors that both take
    /// <see cref="DbContextOptions{TContext}"/>, which the EF tools cannot
    /// disambiguate — this picks the tenant-aware one explicitly with an empty
    /// tenant (design time issues no queries, so the tenant filter is moot).
    /// </summary>
    public sealed class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlServer(config.GetConnectionString("DefaultConnection"))
                .Options;

            return new DataContext(options, new EmptyTenantContext());
        }

        private sealed class EmptyTenantContext : ITenantContext
        {
            public Guid TenantId => Guid.Empty;
            public bool HasTenant => false;
        }
    }
}
