using iTextSharp.text.html;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Services.Common
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly IConfiguration _configuration;

        public MaintenanceService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> BackupDatabaseAsync()
        {
            var backupFolder = @"D:\DATABASE_BACKUP";
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }
            var backupFileName = $"BUTS_ERP_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            var backupFilePath = Path.Combine(backupFolder, backupFileName);

            var backupQuery = $"BACKUP DATABASE [ERP_COA] TO DISK = '{backupFilePath}'";

            try
            {
                string[] files = Directory.GetFiles(backupFolder);
                // Loop through the files and delete each one
                foreach (var file in files)
                    File.Delete(file);
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (var connection = new SqlConnection(connectionString))
                using (var command = new SqlCommand(backupQuery, connection))
                {
                    command.CommandTimeout = 1200; // Set the timeout to 20 minutes (1200 seconds)
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();
                }

                return backupFilePath;
            }
            catch (Exception ex)
            {
                throw new Exception("Database backup failed.", ex);
            }
        }
    }
}
