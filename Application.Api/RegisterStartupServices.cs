using Application.Api.Extensions;
using Application.Api.Interceptors;
using Application.Core.Data;
using Application.Core.Settings;
using Application.Services.Services.Accounts;
using Application.Services.Services.Configuration;
using Application.Services.Services.Productions;
using Application.Services.Services.Purchase;
using Application.Services.Services.Sale;
using Application.Services.Validators.Auth;
using AspNetCoreRateLimit;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Configuration;
using System.Reflection;

public static class RegisterStartupServices
{
    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        // Command timeout is deliberately generous — a dev box running a full copy of
        // production data (tens of thousands of rows on the big tables) has slow
        // reports/dashboards. Override with DatabaseSettings:CommandTimeoutSeconds.
        var commandTimeout = builder.Configuration.GetValue<int?>("DatabaseSettings:CommandTimeoutSeconds") ?? 180;
        builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sql => sql.CommandTimeout(commandTimeout)));
        //  builder.Services.AddTenantDbContext(builder.Configuration);
        //autofac
        builder.Host.AddAutofac();
        //serilog 
        builder.Host.AddSerilog();
        //app settings
        builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
        builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));
        //Rate Limiting
        builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
        builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

        //For In-Memory Caching
        builder.Services.AddMemoryCache();
        //cors
        builder.Services.AddCorsSetup(builder.Configuration);
        //signalr
        builder.Services.AddSignalR();
        //auto mapper
        MapperConfiguration mapperConfiguration = new MapperConfiguration(config =>
        {
            config.AddProfile<ConfigurationMappingProfile>();
            config.AddProfile<PurchaseMappingProfile>();
            config.AddProfile<ProductionMappingProfile>();
            config.AddProfile<SaleMappingProfile>();
            config.AddProfile<AccountMappingProfile>();
        });
        builder.Services.AddSingleton(mapperConfiguration.CreateMapper());
        //add controllers
        builder.Services.AddControllers();
        //Fluent Validation start
        builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
        // Load assemblies that contain validators
        builder.Services.AddValidatorsFromAssembly(Assembly.Load("Application.Services"));
        builder.Services.AddTransient<IValidatorInterceptor, ValidatorInterceptor>();
        //Fluent Validation end
        builder.Services.AddEndpointsApiExplorer();
        //swagger
        builder.Services.AddSwagger();
        return builder;
    }
}

