namespace Application.Api.Extensions
{
    public static class CorsExtension
    {
        public static void AddCorsSetup(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsPolicy", policy =>
                {
                    policy
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials()
                       .WithOrigins(configuration.GetSection("AllowedOrigins").Value.Split(","));
                });
            });

        }
    }
}
