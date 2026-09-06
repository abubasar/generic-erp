using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Collections.ObjectModel;
using System.Data;

namespace Application.Api.Extensions
{
    public static class SerilogExtension
    {
        public static void AddSerilog(this ConfigureHostBuilder host)
        {
            var columnOptions = new ColumnOptions
            {
                AdditionalColumns = new Collection<SqlColumn>
               {
                   new SqlColumn("UserId", SqlDbType.NVarChar),
                   new SqlColumn("TenantId", SqlDbType.UniqueIdentifier),
                   new SqlColumn("Domain", SqlDbType.NVarChar)
               }
            };
            host.UseSerilog((context, configuration) =>
            {
                configuration.Enrich.FromLogContext()
                .WriteTo.MSSqlServer(context.Configuration.GetConnectionString("DefaultConnection"),
                 sinkOptions: new MSSqlServerSinkOptions { TableName = "Log" },
                 null, null, LogEventLevel.Error, null, columnOptions: columnOptions, null, null);
            });
        }
    }
}
