using Application.Api.Middlewares;
using Application.Core.PermissionHelpers;
using Application.Services.Services.Platform;
using AspNetCoreRateLimit;
using Serilog;
using Serilog.Context;

public static class RegisterStartupMiddlewares
{
    public static WebApplication SetupMiddleware(this WebApplication app)
    {
        //|| app.Environment.IsProduction()
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Application.Api v1"));
        }
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<JwtMiddleware>();
        app.UseMiddleware<TenantResolutionMiddleware>();
        app.UseMiddleware<PlatformAuthMiddleware>();

        // First-run: create the seed platform admin from PlatformAuth config if the table is empty.
        using (var scope = app.Services.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IPlatformAuthService>()
                 .EnsureSeedAdminAsync().GetAwaiter().GetResult();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("CorsPolicy");
        app.UseStaticFiles(new StaticFileOptions()
        {
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                ctx.Context.Response.Headers.Append("Access-Control-Allow-Headers",
                  "Origin, X-Requested-With, Content-Type, Accept");
            }
        });
        //(serilog) below code is needed to get User id, tenant id and domain for Log  
        app.Use(async (httpContext, next) =>
        {
            LogContext.PushProperty("UserId", httpContext.GetUserId()); //Push user in LogContext;
            LogContext.PushProperty("TenantId", httpContext.GetTenantId()); //Push tenant in LogContext; 
            LogContext.PushProperty("Domain", httpContext.GetDomain()); //Push tenant in LogContext; 
            await next.Invoke();
        }
       );
        app.UseSerilogRequestLogging();
        //serilog code ended
        app.MapControllers();
        app.UseIpRateLimiting();
        //comment out to implement notification
        // app.MapHub<BroadcastHub>("/notify");
        return app;
    }
}

