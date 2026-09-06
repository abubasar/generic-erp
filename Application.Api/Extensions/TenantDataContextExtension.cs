using Application.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Api
{
    public static class TenantDataContextExtensions
    {

        public static void AddTenantDbContext(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped(provider =>
            {
                var httpContext = provider.GetService<IHttpContextAccessor>();
                if (httpContext?.HttpContext is null)
                {
                    throw new ApplicationException();
                }
                var hasReferer = httpContext.HttpContext.Request.Headers.TryGetValue("Referer", out var referer);
                if (hasReferer)
                {
                    string subdomain = new System.Uri(referer.First()).Host.Split('.')[0].ToUpperInvariant();
                    var opts = new DbContextOptionsBuilder<DataContext>();
                    opts.UseSqlServer(configuration.GetConnectionString("TemplateConnection").Replace("__DBNAME__", subdomain));
                    return new DataContext(opts.Options);
                }
                else
                {
                    var opts = new DbContextOptionsBuilder<DataContext>();
                    opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                    return new DataContext(opts.Options);
                }
            });
        }
    }
}
